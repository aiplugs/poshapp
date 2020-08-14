using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace Aiplugs.PoshApp.Deamon.PowerShell
{
    using PowerShell = System.Management.Automation.PowerShell;
    public static class PowerShellExtensions
    {
        public static IEnumerable<PSObject> InvokeWithParameters(this PowerShell ps, string script, Dictionary<string, object> parameters)
        {
            ps.AddScript(script);

            if (parameters != null)
            {
                ps.AddParameters(parameters);
            }

            return ps.Invoke();
        }

        public static IEnumerable<PSObject> InvokeWithPipeline(this PowerShell ps, string script, object inputObject)
        {
            var parameters = ps.GetParameters(script);
            var valueFromPipeline = parameters.FirstOrDefault(p => p.ValueFromPipeline);
            
            ps.AddScript(script);
            ps.AddParameter(valueFromPipeline.Name, inputObject);

            return ps.Invoke();
        }

        public static IEnumerable<PSParameterInfo> GetParameters(this PowerShell ps, string script)
        {
            var ast = Parser.ParseInput(script, out var tokens, out var errors);
            
            if (ast.ParamBlock == null)
                return Enumerable.Empty<PSParameterInfo>();

            var block = ast.ParamBlock;
            var defaultParameterSetName = ExtractString(block.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "CmdletBinding"), "DefaultParameterSetName");

            return block.Parameters.Select(paramAst =>
            {
                var info = new PSParameterInfo();

                info.DefaultParameterSetName = defaultParameterSetName;
                info.Name = paramAst.Name.VariablePath.UserPath;
                info.ClrType = paramAst.StaticType;

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
                    info.ValidateSet = validateSetAttr.PositionalArguments.Select(arg => (string)arg.SafeGetValue()).ToArray();
                }

                var validateCountAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateCount");
                if (validateCountAttr != null)
                {
                    if (validateCountAttr.PositionalArguments.Count > 0)
                        info.ValidateCount = new[] { ExtractInt(validateCountAttr, 0), ExtractInt(validateCountAttr, 1) };

                    if (validateCountAttr.NamedArguments.Count > 0)
                        info.ValidateCount = new[] { ExtractInt(validateCountAttr, "MinLength"), ExtractInt(validateCountAttr, "MaxLength") };
                }

                var validateLengthAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateLength");
                if (validateLengthAttr != null)
                {
                    if (validateLengthAttr.PositionalArguments.Count > 0)
                        info.ValidateLength = new[] { ExtractInt(validateLengthAttr, 0), ExtractInt(validateLengthAttr, 1) };

                    if (validateLengthAttr.NamedArguments.Count > 0)
                        info.ValidateLength = new[] { ExtractInt(validateLengthAttr, "MinLength"), ExtractInt(validateLengthAttr, "MaxLength") };
                }

                var validateRangeAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidateRange");
                if (validateRangeAttr != null)
                {
                    if (validateRangeAttr.PositionalArguments.Count > 0)
                        info.ValidateRange = new[] { ExtractLong(validateRangeAttr, 0), ExtractLong(validateRangeAttr, 1) };

                    if (validateRangeAttr.NamedArguments.Count > 0)
                        info.ValidateRange = new[] { ExtractLong(validateRangeAttr, "MinRange"), ExtractLong(validateRangeAttr, "MaxRange") };
                }

                var validatePatternAttr = (AttributeAst)paramAst.Attributes.FirstOrDefault(attr => attr.TypeName.Name == "ValidatePattern");
                if (validatePatternAttr != null)
                {
                    info.ValidatePattern = ExtractString(validatePatternAttr, 0);
                }

                return info;
            });
        }
        private static string ExtractString(AttributeAst attr, string paramName)
        {
            return (string)attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName)?.Argument.SafeGetValue();
        }
        private static string ExtractString(AttributeAst attr, int position)
        {
            return (string)attr?.PositionalArguments.ElementAtOrDefault(position)?.SafeGetValue();
        }
        private static bool ExtractBoolean(AttributeAst attr, string paramName)
        {
            return Convert.ToBoolean(attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName)?.Argument.SafeGetValue() ?? default(bool));
        }
        private static int ExtractInt(AttributeAst attr, string paramName)
        {
            return (int)(attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName)?.Argument.SafeGetValue() ?? default(int));
        }
        private static int ExtractInt(AttributeAst attr, int position)
        {
            return (int)(attr?.PositionalArguments.ElementAtOrDefault(position)?.SafeGetValue() ?? default(int));
        }
        private static long ExtractLong(AttributeAst attr, string paramName)
        {
            return Convert.ToInt64(attr?.NamedArguments.FirstOrDefault(a => a.ArgumentName == paramName)?.Argument.SafeGetValue() ?? default(long));
        }
        private static long ExtractLong(AttributeAst attr, int position)
        {
            return Convert.ToInt64(attr?.PositionalArguments.ElementAtOrDefault(position)?.SafeGetValue() ?? default(long));
        }
    }
}
