using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aiplugs.PoshApp.Services
{
    public class ConfigAccessor
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly JsonConverter[] _converters = new[] { new ScriptConverter() };

        public ConfigAccessor(string configDir)
        {
            AppPath = configDir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".poshapp");
            ConfigPath = Path.Combine(AppPath, "config.json");
        }

        public string AppPath { get; private set; }
        public string ConfigPath { get; private set; }
        public async Task<RootConfig> LoadRootConfigAsync()
        {
            TouchConfigIfNotExist();

            var json = await File.ReadAllTextAsync(ConfigPath, Encoding.UTF8);

            if (string.IsNullOrEmpty(json))
                return new RootConfig();

            return JsonConvert.DeserializeObject<RootConfig>(json);
        }

        public async Task SaveRootConfigAsync(RootConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            await _semaphore.WaitAsync();
            try
            {
                await File.WriteAllTextAsync(ConfigPath, json, Encoding.UTF8);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RepositoryConfig> LoadRepositoryConfigAsync(Repository repository)
        {
            if (string.IsNullOrEmpty(repository?.Path))
                return null;

            var configPath = GetConfigPath(repository);

            if (!File.Exists(configPath))
                return new RepositoryConfig();

            var json = await File.ReadAllTextAsync(configPath, Encoding.UTF8);

            return JsonConvert.DeserializeObject<RepositoryConfig>(json, _converters);
        }

        public async Task SaveConfigAsync(Repository repository, RepositoryConfig config)
        {
            var configPath = GetConfigPath(repository);
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            await _semaphore.WaitAsync();
            try
            {
                await File.WriteAllTextAsync(configPath, json, Encoding.UTF8);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void CreateDirIfNotExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        private void TouchFileIfNotExist(string path)
        {
            if (!File.Exists(path))
                File.WriteAllBytes(path, new byte[0]);
        }
        private void TouchConfigIfNotExist()
        {
            CreateDirIfNotExist(AppPath);
            TouchFileIfNotExist(ConfigPath);
        }

        private string GetConfigPath(Repository repository) => Path.Combine(repository.Path, "config.json");

        public class ScriptConverter : JsonConverter
        {
            private readonly static Type _scriptType = typeof(Script);
            public override bool CanConvert(Type objectType)
                => _scriptType == objectType;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return Script.From(serializer.Deserialize<JObject>(reader));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}