using Aiplugs.PoshApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Services
{
    public sealed class LicenseService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ConfigAccessor _configAccessor;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private ActivationStatus? _status = null;

        public LicenseService(ConfigAccessor configAccessor)
        {
            _configAccessor = configAccessor;
        }

        public async Task<ActivationStatus> RegisterActivationCodeAsync(string activationCode)
        {
            var status = await GetActivationStatusImplAsync(activationCode);

            if (status == ActivationStatus.Valid)
            {
                var config = await _configAccessor.LoadRootConfigAsync();
                
                config.ActivationCode = activationCode;

                await _configAccessor.SaveRootConfigAsync(config);

                await _semaphore.WaitAsync();
                try
                {
                    _status = status;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return status;
        }

        public async Task<ActivationStatus> GetActivationStatusAsync()
        {
            if (_status.HasValue)
                return _status.Value;

            await _semaphore.WaitAsync();

            try
            {
                var config = await _configAccessor.LoadRootConfigAsync();
                var status = await GetActivationStatusImplAsync(config.ActivationCode);
                if (status == ActivationStatus.Expired)
                {
                    var refleshed =  await RefleshAsync();
                    if (refleshed != ActivationStatus.None)
                        status = refleshed;
                }
                _status = status;
            }
            finally
            {
                _semaphore.Release();
            }

            return _status.Value;
        }
        private async Task<ActivationStatus> GetActivationStatusImplAsync(string activationCode)
        {
            if (string.IsNullOrEmpty(activationCode))
                return ActivationStatus.None;

            var (result, payload) = await ParseActivationCodeAsync(activationCode);

            if (!result)
                return ActivationStatus.Illigal;

            try
            {
                var o = JObject.Parse(payload);

                var expires = DateTimeOffset.FromUnixTimeSeconds((long)o["exp"]);
                var mac = (string)o["com.aiplugs.poshapp.mac"];
                var host = (string)o["com.aiplugs.poshapp.hostname"];

                if (string.IsNullOrEmpty(mac) || string.IsNullOrEmpty(host))
                    return ActivationStatus.Illigal;

                if (DateTimeOffset.Now > expires)
                    return ActivationStatus.Expired;

                if (!(mac == GetMacAddress() && host == GetMachineName()))
                    return ActivationStatus.NotMatch;

                return ActivationStatus.Valid;
            }
            catch(Exception) { }
            return ActivationStatus.Illigal;
        }
        public string GetActivationRequestCode()
        {
            var json = JsonConvert.SerializeObject(new
            {
                ComputerName = GetMachineName(),
                MACAddress = GetMacAddress()
            });

            var bin = Encoding.UTF8.GetBytes(json);

            return Convert.ToBase64String(bin);
        }
        private string GetMachineName()
        {
            return Environment.MachineName;
        }
        private string GetMacAddress()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                var mac = nic.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(mac))
                    return mac;
            }
            return null;
        }
        private async Task<IEnumerable<X509Certificate2>> GetPublicCertAsync(string domain)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadWrite);

            IEnumerable<X509Certificate2> find()
            {
                var now = DateTimeOffset.Now;
                var collection = store.Certificates.Find(X509FindType.FindBySubjectName, domain, true);

                foreach (var cert in collection)
                {
                    if (cert.NotBefore < now && now < cert.NotAfter)
                        yield return cert;
                }
            }

            try
            {
                await new HttpClient(new HttpClientHandler
                {
                    UseDefaultCredentials = true,
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
                    {
                        if (error != SslPolicyErrors.None || !cert.Verify())
                            return false;

                        store.Add(cert);
                        return true;
                    }
                }).SendAsync(new HttpRequestMessage(HttpMethod.Head, $"https://{domain}/"));
                return find();
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async Task<(bool, string)> ParseActivationCodeAsync(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
                return (false, null);

            var splited = jwt.Split('.');
            if (splited.Length != 3)
                return (false, null);

            byte[] fromBase64Url(string base64Url)
            {
                int paddingNum = base64Url.Length % 4;
                if (paddingNum != 0)
                {
                    paddingNum = 4 - paddingNum;
                }
                return Convert.FromBase64String(
                                    (base64Url + new string('=', paddingNum))
                                    .Replace('-', '+')
                                    .Replace('_', '/'));
            }

            try
            {
                var header = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(Convert.FromBase64String(splited[0])));
                var payload = Encoding.UTF8.GetString(fromBase64Url(splited[1]));
                var signature = fromBase64Url(splited[2]);

                if (!(header.typ == "JWT" && header.alg == "RS256"))
                    return (false, null);

                var certs = await GetPublicCertAsync("poshapp.aiplugs.com");

                if (certs == null)
                    return (false, null);

                foreach (var cert in certs)
                {
                    var rsa = cert.GetRSAPublicKey();
                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA256");
                    using var sha = SHA256.Create();
                    var result = rsaDeformatter.VerifySignature(sha.ComputeHash(Encoding.UTF8.GetBytes($"{splited[0]}.{splited[1]}")), signature);

                    if (!result)
                        continue;

                    return (result, payload);
                }

                return (false, null);
            }
            catch
            {
                return (false, null);
            }
        }
        private async Task<string> RefleshAsync(string activationCode)
        {
            var response = await _httpClient.PostAsync("https://poshapp.aiplugs.com/api/reflesh", new StringContent(JsonConvert.SerializeObject(new { activationCode }), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<dynamic>(json).code;
        }
        public async Task<ActivationStatus> RefleshAsync()
        {
            var config = await _configAccessor.LoadRootConfigAsync();

            var code = await RefleshAsync(config.ActivationCode);

            var status = await GetActivationStatusImplAsync(code);
            if (status == ActivationStatus.Valid)
            {
                config.ActivationCode = code;
                await _configAccessor.SaveRootConfigAsync(config);
            }

            return status;
        }
    }
}
