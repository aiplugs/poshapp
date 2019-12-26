using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;

namespace Aiplugs.PoshApp.Services
{
    public abstract class InvokeCommand 
    {
        public string ConnectionId { get; set; }
        public string ScriptId { get; set; }
    }
    public class DefaultCommand : InvokeCommand
    {
        public Dictionary<string, object> Parameters { get; set; }
    }
    public class GetParametersCommand : InvokeCommand
    {
    }
    public class ListInvokeCommand : InvokeCommand
    {
    }
    public class DetailInvokeCommand : InvokeCommand
    {
        public object InputObject { get; set; }
    }
    public class SingletonInvokeCommand : InvokeCommand
    {
    }
    public class ActionInvokeCommand : InvokeCommand
    {
        public object InputObject { get; set; }
    }

    public class PowershellIO
    {
        public readonly ConcurrentQueue<Dictionary<string, PSObject>> PromptQueue = new ConcurrentQueue<Dictionary<string, PSObject>>();
        public readonly ConcurrentQueue<int> ChoiceQueue = new ConcurrentQueue<int>();
        public readonly ConcurrentQueue<PSCredential> CredentialQueue = new ConcurrentQueue<PSCredential>();
        public readonly ConcurrentQueue<string> LineQueue = new ConcurrentQueue<string>();
    }
}