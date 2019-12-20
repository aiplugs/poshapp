using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aiplugs.PoshApp
{
    public class ConfigAccessor
    {
        private readonly JsonConverter[] _converters = new[] { new ScriptConverter() };
        private readonly IMemoryCache _cache;
        private readonly static string CACHE_KEY = typeof(ConfigAccessor).FullName;
        public ConfigAccessor(IMemoryCache cache)
        {
            _cache = cache;
        }
        public async Task<Config> LoadConfigAsync()
        {
            return await _cache.GetOrCreateAsync(CACHE_KEY, async entry =>
            {
                StorageHelper.TouchConfigIfNotExist();

                var json = await File.ReadAllTextAsync(StorageHelper.GetConfigPath(), Encoding.UTF8);

                if (string.IsNullOrEmpty(json))
                    return new Config();
                
                return JsonConvert.DeserializeObject<Config>(json, _converters);
            });
        }
        public async Task SaveConfigAsync(Config config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            await File.WriteAllTextAsync(StorageHelper.GetConfigPath(), json, Encoding.UTF8);
            _cache.Set(CACHE_KEY, config);
        }

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