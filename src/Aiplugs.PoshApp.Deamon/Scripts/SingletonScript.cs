using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Models
{
    public class SingletonScript : Script, IActionTarget
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("actions")]
        public string[] Actions { get; set; }

        public SingletonScript()
        {
            Type = ScriptType.Singleton;
        }
    }
}