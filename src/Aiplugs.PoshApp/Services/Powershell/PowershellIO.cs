using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;

namespace Aiplugs.PoshApp.Services.Powersehll
{

    public class PowershellIO
    {
        public readonly ConcurrentQueue<Dictionary<string, PSObject>> PromptQueue = new ConcurrentQueue<Dictionary<string, PSObject>>();
        public readonly ConcurrentQueue<int> ChoiceQueue = new ConcurrentQueue<int>();
        public readonly ConcurrentQueue<PSCredential> CredentialQueue = new ConcurrentQueue<PSCredential>();
        public readonly ConcurrentQueue<string> LineQueue = new ConcurrentQueue<string>();
    }
}