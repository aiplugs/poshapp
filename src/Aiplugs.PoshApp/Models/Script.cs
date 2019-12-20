using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Aiplugs.PoshApp.Models
{
    public abstract class Script
    {
        [JsonProperty("id")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9]$")]
        public string Id { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScriptType Type { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        public static Script From(JObject jObject)
        {
            var type = ((string)jObject["type"] ?? string.Empty).ToLower();

            if (type == nameof(ScriptType.List).ToLower())
                return jObject.ToObject<ListScript>();

            if (type == nameof(ScriptType.Detail).ToLower())
                return jObject.ToObject<DetailScript>();

            if (type == nameof(ScriptType.Action).ToLower())
                return jObject.ToObject<ActionScript>();

            if (type == nameof(ScriptType.Singleton).ToLower())
                return jObject.ToObject<SingletonScript>();

            throw new NotSupportedException($"Script type = '{type}' is not supported.");
        }
    }
}