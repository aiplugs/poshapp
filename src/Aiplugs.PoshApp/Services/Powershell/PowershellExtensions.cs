using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Aiplugs.PoshApp.Services.Powersehll
{
    public static class PowershellExtensions
    {
        public static IEnumerable<PSObject> InvokeWithParameters(this PowerShell ps, string script, Dictionary<string, object> parameters)
        {
            ps.AddScript($"Function Invoke-PoshApp \n{{\n{script}\n}}").Invoke();
            ps.Commands.Clear();
            ps.AddCommand("Invoke-PoshApp");
            
            if (parameters != null) {
                ps.AddParameters(parameters);
            }
            
            return ps.Invoke();
        }

        public static IEnumerable<PSObject> InvokeWithPipeline(this PowerShell ps, string script, object inputObject) 
        {
            var parameters = ps.GetParameters(script);
            var valueFromPipeline = parameters.FirstOrDefault(p => p.ValueFromPipeline);
            ps.AddScript($"Function Invoke-PoshApp \n{{\n{script}\n}}").Invoke();
            ps.Commands.Clear();
            ps.AddCommand($"Invoke-PoshApp").AddParameter(valueFromPipeline.Name, inputObject);
            return ps.Invoke();
        }
        public static IEnumerable<PSParameterInfo> GetParameters(this PowerShell ps, string script)
        {
            var query = $"{{\n{script}\n}}.Ast.ParamBlock";
            
            ps.AddScript(query);
            var results = ps.Invoke();

            if (results.Count == 0 || results[0] == null)
                return Enumerable.Empty<PSParameterInfo>();

            var block = (ParamBlockAst)results[0].ImmediateBaseObject;
            var defaultParameterSetName = ExtractString(block.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "CmdletBinding"), "DefaultParameterSetName");
            
            return block.Parameters.Select(paramAst => 
            {
                var info = new PSParameterInfo();
                
                info.DefaultParameterSetName = defaultParameterSetName;
                info.Name = paramAst.Name.VariablePath.UserPath;
                info.Type = paramAst.StaticType;
                
                var paramAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "Parameter");

                if (paramAttr != null) 
                {
                    info.Mandatory = ExtractBoolean(paramAttr, "Mandatory");
                    info.DontShow = ExtractBoolean(paramAttr, "DontShow");
                    info.HelpMessage = ExtractString(paramAttr, "HelpMessage");
                    info.HelpMessageBaseName = ExtractString(paramAttr, "HelpMessageBaseName");
                    info.HelpMessageResourceId = ExtractString(paramAttr, "HelpMessageResourceId");
                    info.ParameterSetName = ExtractString(paramAttr, "ParameterSetName");
                    info.Position = ExtractInt(paramAttr, "Position");
                    info.ValueFromPipeline = ExtractBoolean(paramAttr, "ValueFromPipeline");
                    info.ValueFromPipelineByPropertyName = ExtractBoolean(paramAttr, "ValueFromPipelineByPropertyName");
                    info.ValueFromRemainingArguments = ExtractBoolean(paramAttr, "ValueFromRemainingArguments");
                    info.DefaultValue = ExtractString(paramAttr, "DefaultValue");
                }

                var validateSetAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateSet");
                if (validateSetAttr != null) 
                {
                    info.ValidateSet = validateSetAttr.PositionalArguments.Select(arg => arg.Extent.Text).ToArray();
                }

                var validateCountAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateCount");
                if (validateCountAttr != null) 
                {
                }

                var validateLengthAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateLength");
                if (validateLengthAttr != null) 
                {
                }

                var validateRangeAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateRange");
                if (validateRangeAttr != null) 
                {
                }

                var validatePatternAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidatePattern");
                if (validatePatternAttr != null) 
                {
                }

                return info;
            });
        }
        private static string ExtractString(AttributeAst attr, string paramName)
            => attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName)?.Argument.Extent.Text;
        private static bool ExtractBoolean(AttributeAst attr, string paramName)
        {
            var arg = attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName);

            if (arg == null)
                return default(bool);

            return Convert.ToBoolean(((VariableExpressionAst)arg.Argument).VariablePath.UserPath);
        }
        private static int ExtractInt(AttributeAst attr, string paramName)
        {
            var arg = attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName);

            if (arg == null)
                return default(int);

            return Convert.ToInt32(((VariableExpressionAst)arg.Argument).VariablePath.UserPath);
        }
    }
    public class PSParameterInfo
    {
        public string Name { get; set; }
        public Type Type { get; set; }
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
        public string[] ValidateSet { get;set; }
    }

    public class PSInvokeResult
    {
        public ICollection<PSObject> Output { get; } = new List<PSObject>();
        public ICollection<PSObject> Information { get; } = new List<PSObject>();
        public ICollection<PSObject> Warning { get; } = new List<PSObject>();
        public ICollection<PSObject> Progress { get; } = new List<PSObject>();
    }
}