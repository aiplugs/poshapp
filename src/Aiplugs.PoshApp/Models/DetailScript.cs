using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Models
{
    public class DetailScript : Script, IActionTarget
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("actions")]
        public string[] Actions { get; set; }

        public DetailScript()
        {
            Type = ScriptType.Detail;
        }
    }
}