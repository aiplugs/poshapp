using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aiplugs.PoshApp.Services
{
    public class ConfigAccessor
    {
        private readonly JsonConverter[] _converters = new[] { new ScriptConverter() };
        private static object _lock = new object();
        private readonly IMemoryCache _cache;
        public ConfigAccessor(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<RootConfig> LoadRootConfigAsync()
        {
            return await _cache.GetOrCreateAsync("root", async entry =>
            {
                StorageHelper.TouchConfigIfNotExist();

                var json = await File.ReadAllTextAsync(StorageHelper.GetConfigPath(), Encoding.UTF8);

                if (string.IsNullOrEmpty(json))
                    return new RootConfig();

                return JsonConvert.DeserializeObject<RootConfig>(json);
            });
        }
        public Task SaveRootConfigAsync(RootConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            lock(_lock) {
                File.WriteAllText(StorageHelper.GetConfigPath(), json, Encoding.UTF8);
                _cache.Set("root", config);
            }

            return Task.FromResult(0);
        }

        public async Task<Config> LoadConfigAsync(Repository repository)
        {
            if (string.IsNullOrEmpty(repository?.Path))
                return null;

            return await _cache.GetOrCreateAsync(repository.Path, async entry =>
            {
                var configPath = GetConfigPath(repository);

                if (!File.Exists(configPath))
                    return new Config();

                var json = await File.ReadAllTextAsync(configPath, Encoding.UTF8);

                return JsonConvert.DeserializeObject<Config>(json, _converters);
            });
        }

        public Task SaveConfigAsync(Repository repository, Config config)
        {
            if (!string.IsNullOrEmpty(repository.Path))
            {
                var configPath = GetConfigPath(repository);
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                lock(_lock) {
                    File.WriteAllText(configPath, json, Encoding.UTF8);
                    _cache.Set(repository.Path, config);
                }
            }

            return Task.FromResult(0);
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