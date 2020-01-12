using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace Aiplugs.PoshApp.Services.Powersehll
{
    public interface IInternalPowershellUI
    {
        Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions);
        int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice);
        PSCredential PromptForCredential(string caption, string message, string userName, string targetName);
        PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options);
        string ReadLine();
        SecureString ReadLineAsSecureString();
        void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
        void Write(string value);
        void WriteDebugLine(string message);
        void WriteErrorLine(string value);
        void WriteLine(string value);
        void WriteProgress(long sourceId, ProgressRecord record);
        void WriteVerboseLine(string message);
        void WriteWarningLine(string message);
    }
    public class PowershellUI : PSHostUserInterface
    {
        public PowershellUI(IInternalPowershellUI internalPowershellUI, PowershellRawUI rawUI)
        {
            _internalPowershellUI = internalPowershellUI;
            _rawUI = rawUI;
        }

        private readonly IInternalPowershellUI _internalPowershellUI;
        private readonly PSHostRawUserInterface _rawUI;
        public override PSHostRawUserInterface RawUI => _rawUI;

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return _internalPowershellUI.Prompt(caption, message, descriptions);
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return _internalPowershellUI.PromptForChoice(caption, message, choices, defaultChoice);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return _internalPowershellUI.PromptForCredential(caption, message, userName, targetName);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return _internalPowershellUI.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
        }

        public override string ReadLine()
        {
            return _internalPowershellUI.ReadLine();
        }

        public override SecureString ReadLineAsSecureString()
        {
            return _internalPowershellUI.ReadLineAsSecureString();
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _internalPowershellUI.Write(foregroundColor, backgroundColor, value);
        }

        public override void Write(string value)
        {
            _internalPowershellUI.Write(value);
        }

        public override void WriteDebugLine(string message)
        {
           _internalPowershellUI.WriteDebugLine(message);
        }

        public override void WriteErrorLine(string value)
        {
            _internalPowershellUI.WriteErrorLine(value);
        }

        public override void WriteLine(string value)
        {
            _internalPowershellUI.WriteLine(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            _internalPowershellUI.WriteProgress(sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            _internalPowershellUI.WriteVerboseLine(message);
        }

        public override void WriteWarningLine(string message)
        {
            _internalPowershellUI.WriteWarningLine(message);
        }
    }
}