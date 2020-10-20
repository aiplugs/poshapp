using Aiplugs.PoshApp.Deamon.PowerShell;
using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.Services;
using Microsoft.PowerShell;
using Newtonsoft.Json;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Deamon
{
    public class PoshAppService
    {
        private readonly Runspace _runspace;
        private readonly JsonRpc _rpc;
        private readonly ConfigAccessor _config;
        private readonly LicenseService _license;
        private readonly ScriptsService _scripts;

        private PoshAppService(string configDir)
        {
            _config = new ConfigAccessor(configDir);
            _license = new LicenseService(_config);
            _scripts = new ScriptsService(_config, _license);
        }
        public PoshAppService(Stream sendingStream, Stream receivingStream, string configDir = null) : this(configDir)
        {
            _rpc = JsonRpc.Attach(sendingStream, receivingStream, this);
            _runspace = CreateRunspace();
        }
        public PoshAppService(IJsonRpcMessageHandler handler, string configDir = null) : this(configDir)
        {
            _rpc = new JsonRpc(handler, this);
            _rpc.CancelLocallyInvokedMethodsWhenConnectionIsClosed = true;
            _rpc.StartListening();
            _runspace = CreateRunspace();
        }
        internal async Task StartAsync() {
            _runspace.Open();

            await _rpc.Completion;
            
            _runspace.Close();
        }

#pragma warning disable VSTHRD200
        public async Task<string> GetChannel()
        {
            var root = await _config.LoadRootConfigAsync();
            return root.Channel;
        }
        public async Task<IEnumerable<PSObject>> InvokeWithParameters(string scriptId, Dictionary<string, string> parameters)
        {
            return await DoAsync(async () =>
            {
                var content = await _scripts.GetScriptContentAsync(scriptId);
                var workingDir = await _scripts.GetScriptDirAsync(scriptId);

                using var ps = CreatePowerShell(workingDir);
                var args = parameters?.ToDictionary(p => p.Key, p => PSSerializer.Deserialize(p.Value));
                var result = ps.InvokeWithParameters(content, args);
                //await _rpc.NotifyAsync("DefaultResult", JsonConvert.SerializeObject(result));
                //return JsonConvert.SerializeObject(result);
                return result;
            });
        }

        public async Task<IEnumerable<PSObject>> InvokeWithPipeline(string scriptId, string clixml)
        {
            return await DoAsync(async () =>
            {
                var content = await _scripts.GetScriptContentAsync(scriptId);
                var workingDir = await _scripts.GetScriptDirAsync(scriptId);
                using var ps = CreatePowerShell(workingDir);
                var arg = PSSerializer.Deserialize(clixml);
                var result = ps.InvokeWithPipeline(content, arg);
                return result;
            });
        }

        public async Task<IEnumerable<PSObject>> InvokeWithPipelines(string scriptId, string[] clixmls)
        {
            return await DoAsync(async () =>
            {
                var content = await _scripts.GetScriptContentAsync(scriptId);
                var workingDir = await _scripts.GetScriptDirAsync(scriptId);
                using var ps = CreatePowerShell(workingDir);
                var args = clixmls?.Select(clixml => PSSerializer.Deserialize(clixml)).ToArray();
                var result = ps.InvokeWithPipeline(content, args);
                return result;
            });
        }

        public async Task<IEnumerable<PSParameterInfo>> InvokeGetParameters(string scriptId)
        {
            return await DoAsync(async () => {
                var content = await _scripts.GetScriptContentAsync(scriptId);
                var workingDir = await _scripts.GetScriptDirAsync(scriptId);
                using var ps = CreatePowerShell(workingDir);
                var parameters = ps.GetParameters(content);
                return parameters;
            });
        }

        public async Task<object> GetActivation()
        {
            var status = (await _license.GetActivationStatusAsync()).ToString();
            var requestCode = _license.GetActivationRequestCode();

            return new { status, requestCode };
        }

        public async Task<object> PostActivation(string activationCode)
        {
            var status = (await _license.RegisterActivationCodeAsync(activationCode)).ToString();
            var requestCode = _license.GetActivationRequestCode();

            return new { status, requestCode };
        }

        public async Task<object> RefleshActivation()
        {
            await _license.RefleshAsync();
            var status = (await _license.GetActivationStatusAsync()).ToString();
            var requestCode = _license.GetActivationRequestCode();

            return new { status, requestCode };
        }

        public async Task<IEnumerable<Repository>> GetRepositories()
        {
            var repositories = await _scripts.GetRepositoriesAsync();
            return repositories;
        }

        public async Task<HttpStatusCode> CreateRepository(string name, string path)
        {
            if (await _scripts.ExistRepositoryAsync(name))
                return HttpStatusCode.Conflict;

            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(_config.AppPath, name);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            var status = await _license.GetActivationStatusAsync();
            if (status == ActivationStatus.None)
            {
                var repositories = await _scripts.GetRepositoriesAsync();
                if (repositories.Count() > Limitation.FREE_PLAN_MAX_REPOSITORIES)
                    return HttpStatusCode.PaymentRequired;
            }

            await _scripts.AddRepositoryAsync(new Repository { Name = name, Path = path });

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> UpdateRepository(string currentName, string name, string path)
        {
            if (!await _scripts.ExistRepositoryAsync(name))
                return HttpStatusCode.NotFound;

            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(_config.AppPath, name);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            await _scripts.UpdateRepositoryAsync(currentName, new Repository { Name = name, Path = path });

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> DeleteRepository(string name, bool deleteFiles)
        {
            if (!await _scripts.ExistRepositoryAsync(name))
                return HttpStatusCode.NotFound;

            var repository = await _scripts.GetRepositoryAsync(name);

            await _scripts.RemoveRepositoryAsync(name);

            if (deleteFiles)    
                Directory.Delete(repository.Path, true);

            return HttpStatusCode.NoContent;
        }

        public async Task<IReadOnlyDictionary<string, IEnumerable<Script>>> GetScripts()
        {
            return await _scripts.GetScriptListAsync();
        }

        public async Task<Script> GetScript(string repositoryName, string id)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return null;

            return await _scripts.GetScriptAsync(repository, id);
        }

        public async Task<string> GetScriptContent(string repositoryName, string id)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return null;

            var script = await _scripts.GetScriptAsync(repository, id);

            if (script == null)
                return null;

            return await _scripts.GetScriptContentAsync(repository, script);
        }

        public async Task<HttpStatusCode> PutScriptContent(string repositoryName, string id, string content)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            var script = await _scripts.GetScriptAsync(repository, id);

            if (script == null)
                return HttpStatusCode.NotFound;

            await _scripts.UpdateScriptContentAsync(repository, script, content);

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> DeleteScript(string repositoryName, string id)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (!await _scripts.ExistScriptAsync(repository, id))
                return HttpStatusCode.NotFound;

            await _scripts.RemoveScriptAsync(repository, id);

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> CreateListScript(string repositoryName, ListScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            var status = await _license.GetActivationStatusAsync();
            if (status == ActivationStatus.None)
            {
                var scripts = await _scripts.GetScriptListAsync();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return HttpStatusCode.PaymentRequired;
            }

            await _scripts.AddScriptAsync(repository, model);

            return HttpStatusCode.Created;
        }

        public async Task<HttpStatusCode> UpdateListScript(string repositoryName, string id, ListScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (!await _scripts.ExistScriptAsync(repository, id))
                return HttpStatusCode.NotFound;

            if (id != model.Id && await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            await _scripts.UpdateScriptAsync(repository, id, model);

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> CreateDetailScript(string repositoryName, DetailScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            var status = await _license.GetActivationStatusAsync();
            if (status == ActivationStatus.None)
            {
                var scripts = await _scripts.GetScriptListAsync();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return HttpStatusCode.PaymentRequired;
            }

            await _scripts.AddScriptAsync(repository, model);

            return HttpStatusCode.Created;
        }

        public async Task<HttpStatusCode> UpdateDetailScript(string repositoryName, string id, DetailScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (!await _scripts.ExistScriptAsync(repository, id))
                return HttpStatusCode.NotFound;

            if (id != model.Id && await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            await _scripts.UpdateScriptAsync(repository, id, model);

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> CreateSingletonScript(string repositoryName, SingletonScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            var status = await _license.GetActivationStatusAsync();
            if (status == ActivationStatus.None)
            {
                var scripts = await _scripts.GetScriptListAsync();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return HttpStatusCode.PaymentRequired;
            }

            await _scripts.AddScriptAsync(repository, model);

            return HttpStatusCode.Created;
        }

        public async Task<HttpStatusCode> UpdateSingletonScript(string repositoryName, string id, SingletonScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (!await _scripts.ExistScriptAsync(repository, id))
                return HttpStatusCode.NotFound;

            if (id != model.Id && await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            await _scripts.UpdateScriptAsync(repository, id, model);

            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> CreateActionScript(string repositoryName, ActionScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            var status = await _license.GetActivationStatusAsync();
            if (status == ActivationStatus.None)
            {
                var scripts = await _scripts.GetScriptListAsync();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return HttpStatusCode.PaymentRequired;
            }

            await _scripts.AddScriptAsync(repository, model);

            return HttpStatusCode.Created;
        }

        public async Task<HttpStatusCode> UpdateActionScript(string repositoryName, string id, ActionScript model)
        {
            var repository = await _scripts.GetRepositoryAsync(repositoryName);

            if (repository == null)
                return HttpStatusCode.NotFound;

            if (!await _scripts.ExistScriptAsync(repository, id))
                return HttpStatusCode.NotFound;

            if (id != model.Id && await _scripts.ExistScriptAsync(repository, model.Id))
                return HttpStatusCode.Conflict;

            await _scripts.UpdateScriptAsync(repository, id, model);

            return HttpStatusCode.NoContent;
        }
#pragma warning restore VSTHRD200

        private async Task<TResult> DoAsync<TResult>(Func<Task<TResult>> action) where TResult : class
        {
            try
            {
                return await action();
            }
            catch (ParseException parseException)
            {
                await _rpc.NotifyAsync("ParseError", parseException.Message);
                await _rpc.NotifyAsync("UnitResult");
            }
            catch (Exception exception)
            {
                await _rpc.NotifyAsync("WriteErrorLine", exception.Message);
                await _rpc.NotifyAsync("UnitResult");
            }
            return null;
        }

        private Runspace CreateRunspace()
        {
            var host = new Host(new UserInterface(_rpc));

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var iss = InitialSessionState.CreateDefault();
                iss.ExecutionPolicy = ExecutionPolicy.Unrestricted;
                return RunspaceFactory.CreateRunspace(host, iss);
            }

            return RunspaceFactory.CreateRunspace(host);
        }

        private System.Management.Automation.PowerShell CreatePowerShell(string workingDir)
        {
            _runspace.SessionStateProxy.Path.SetLocation(workingDir);
            var powershell = System.Management.Automation.PowerShell.Create(_runspace);
            powershell.Streams.Error.DataAdding += (sender, args) =>
            {
                var record = (ErrorRecord)args.ItemAdded;
                var message = record.ToString();
                _rpc.NotifyAsync("WriteErrorLine", message).ConfigureAwait(false).GetAwaiter().GetResult();
            };
            return powershell;
        }
    }
}
