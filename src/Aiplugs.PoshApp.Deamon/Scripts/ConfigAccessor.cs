using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aiplugs.PoshApp.Services
{
    public class ConfigAccessor
    {
        private static readonly string _appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".poshapp");
        private static readonly string _configPath = Path.Combine(_appPath, "config.json");
        private readonly JsonConverter[] _converters = new[] { new ScriptConverter() };
        public async Task<RootConfig> LoadRootConfigAsync()
        {
            TouchConfigIfNotExist();

            var json = await File.ReadAllTextAsync(_configPath, Encoding.UTF8);

            if (string.IsNullOrEmpty(json))
                return new RootConfig();

            return JsonConvert.DeserializeObject<RootConfig>(json);
        }

        public async Task SaveRootConfigAsync(RootConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            await File.WriteAllTextAsync(_configPath, json, Encoding.UTF8);
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
            await File.WriteAllTextAsync(configPath, json, Encoding.UTF8);
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
            CreateDirIfNotExist(_appPath);
            TouchFileIfNotExist(_configPath);
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