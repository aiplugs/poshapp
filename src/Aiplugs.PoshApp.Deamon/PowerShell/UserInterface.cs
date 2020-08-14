using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace Aiplugs.PoshApp.Deamon
{
    public class UserInterface : PSHostUserInterface
    {
        private readonly JsonRpc _rpc;
        public UserInterface(JsonRpc rpc)
        {
            _rpc = rpc;
        }
        private readonly PSHostRawUserInterface _rawUI = new RawUserInterface();
        public override PSHostRawUserInterface RawUI => _rawUI;

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            var result = _rpc.InvokeAsync<Dictionary<string, string>>("Prompt", caption, message, descriptions).ConfigureAwait(false).GetAwaiter().GetResult();

            return result.ToDictionary(o => o.Key, o =>
            {
                if (o.Value == null)
                    return null;

                var value = PSSerializer.Deserialize(o.Value);
                if (value == null)
                    return null;

                return new PSObject(value);
            });
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return _rpc.InvokeAsync<int>("PromptForChoice", caption, message, choices).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            var result = _rpc.InvokeAsync<Credential>("PromptForCredential", caption, message, userName, targetName).ConfigureAwait(false).GetAwaiter().GetResult();
            var secure = new SecureString();
            foreach (var c in result.Password.ToCharArray())
            {
                secure.AppendChar(c);
            }
            return new PSCredential(result.Username, secure);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            var result = _rpc.InvokeAsync<Credential>("PromptForCredential", caption, message, userName, targetName).ConfigureAwait(false).GetAwaiter().GetResult();
            var secure = new SecureString();
            foreach (var c in result.Password.ToCharArray())
            {
                secure.AppendChar(c);
            }
            return new PSCredential(result.Username, secure);
        }

        public override string ReadLine()
        {
            return _rpc.InvokeAsync<string>("ReadLine").ConfigureAwait(false).GetAwaiter().GetResult();

        }

        public override SecureString ReadLineAsSecureString()
        {
            var line = _rpc.InvokeAsync<string>("ReadLineAsSecureString").ConfigureAwait(false).GetAwaiter().GetResult();
            var secure = new SecureString();
            foreach (var c in line.ToCharArray())
            {
                secure.AppendChar(c);
            }
            return secure;
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _rpc.NotifyAsync("Write", foregroundColor, backgroundColor, value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void Write(string value)
        {
            _rpc.NotifyAsync("Write", value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void WriteDebugLine(string message)
        {
            _rpc.NotifyAsync("WriteDebugLine", message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void WriteErrorLine(string value)
        {
            _rpc.NotifyAsync("WriteErrorLine", value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void WriteLine(string value)
        {
            _rpc.NotifyAsync("WriteLine", value).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            _rpc.NotifyAsync("WriteProgress", sourceId, record).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void WriteVerboseLine(string message)
        {
            _rpc.NotifyAsync("WriteVerboseLine", message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void WriteWarningLine(string message)
        {
            _rpc.NotifyAsync("WriteWarningLine", message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public class Credential
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
