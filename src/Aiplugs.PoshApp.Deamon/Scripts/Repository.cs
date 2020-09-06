using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Models
{
    public class Repository
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}