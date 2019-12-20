using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Models
{
    public class ListScript : Script, IActionTarget
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("actions")]
        public string[] Actions { get; set; }

        public ListScript() 
        {
            Type = ScriptType.List;
        }
    }
}