using System;

namespace Aiplugs.PoshApp.Deamon.PowerShell
{
    public class PSParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        internal Type ClrType { get; set; }
        public bool Mandatory { get; set; }
        public bool DontShow { get; set; }
        public string HelpMessage { get; set; }
        public string HelpMessageBaseName { get; set; }
        public string HelpMessageResourceId { get; set; }
        public string ParameterSetName { get; set; }
        public int Position { get; set; }
        public bool ValueFromPipeline { get; set; }
        public bool ValueFromPipelineByPropertyName { get; set; }
        public bool ValueFromRemainingArguments { get; set; }
        public string DefaultParameterSetName { get; set; }
        public string DefaultValue { get; set; }
        public string[] ValidateSet { get; set; }
        public int[] ValidateCount { get; set; }
        public int[] ValidateLength { get; set; }
        public long[] ValidateRange { get; set; }
        public string ValidatePattern { get; set; }
    }
}
