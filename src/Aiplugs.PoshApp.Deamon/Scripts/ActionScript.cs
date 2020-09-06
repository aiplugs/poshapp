using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Models
{
    public class ActionScript : Script
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        public ActionScript()
        {
            Type = ScriptType.Action;
        }
    }
}