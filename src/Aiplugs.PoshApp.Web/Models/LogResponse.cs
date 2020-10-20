using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Aiplugs.PoshApp.Web.Models
{
    public class LogResponse
    {
        [JsonProperty("logs")]
        public IEnumerable<LogEntry> Logs { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("local")]
        public string Local { get; set; }
    }

    public class LogEntry
    {
        [JsonProperty("commit")]
        public string Commit { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("when")]
        public DateTimeOffset When { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("messageShort")]
        public string MessageShort { get; set; }
    }
}
