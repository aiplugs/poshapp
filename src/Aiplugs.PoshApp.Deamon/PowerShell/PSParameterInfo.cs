using Newtonsoft.Json;
using System;

namespace Aiplugs.PoshApp.Deamon.PowerShell
{
    public class PSParameterInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type => ClrType.FullName;
        
        internal Type ClrType { get; set; }
        
        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; }
        
        [JsonProperty("dontShow")]
        public bool DontShow { get; set; }
        
        [JsonProperty("helpMessage")]
        public string HelpMessage { get; set; }
        
        [JsonProperty("helpMessageBaseName")]
        public string HelpMessageBaseName { get; set; }
        
        [JsonProperty("helpMessageResourceId")]
        public string HelpMessageResourceId { get; set; }
        
        [JsonProperty("parameterSetName")]
        public string ParameterSetName { get; set; }
        
        [JsonProperty("position")]
        public int Position { get; set; }
        
        [JsonProperty("valueFromPipeline")]
        public bool ValueFromPipeline { get; set; }
        
        [JsonProperty("valueFromPipelineByPropertyName")]
        public bool ValueFromPipelineByPropertyName { get; set; }
        
        [JsonProperty("valueFromRemainingArguments")]
        public bool ValueFromRemainingArguments { get; set; }
        
        [JsonProperty("defaultParameterSetName")]
        public string DefaultParameterSetName { get; set; }
        
        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }
        
        [JsonProperty("validateSet")]
        public string[] ValidateSet { get; set; }
        
        [JsonProperty("validateCount")]
        public int[] ValidateCount { get; set; }
        
        [JsonProperty("validateLength")]
        public int[] ValidateLength { get; set; }
        
        [JsonProperty("validateRange")]
        public long[] ValidateRange { get; set; }
        
        [JsonProperty("validatePattern")]
        public string ValidatePattern { get; set; }
    }
}
