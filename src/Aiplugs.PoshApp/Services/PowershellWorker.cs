using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerShell;
using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Services
{
    public class PowershellWorker : BackgroundService, IInternalPowershellUI
    {
        private readonly ScriptsService _service;
        private readonly IHubContext<PoshAppHub> _hub;
        private readonly PowershellContext _context;
        public PowershellWorker(ScriptsService service, IHubContext<PoshAppHub> hub, PowershellContext context)
        {
            _service = service;
            _hub = hub;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var host = new PowershellHost(new PowershellUI(this, new PowershellRawUI()));
                var iss = InitialSessionState.CreateDefault();
                
                using var runspace = RunspaceFactory.CreateRunspace(host, iss);
                
                runspace.Open();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await TryInvokeCommand(runspace);

                    await WaitAndCheckCancel(1000);
                }

                runspace.Close();
            } 
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }
        private async Task TryInvokeCommand(Runspace runspace)
        {
            if (_context.TryDequeueCommand(out var invokeCommand))
            {
                CurrentConnectionId = invokeCommand.ConnectionId;

                var content = await _service.GetScriptContent(invokeCommand.ScriptId);
                if (content == null)
                    return;

                using var ps = PowerShell.Create();
                ps.Streams.Error.DataAdding += (sender, args) => {
                    var record = (ErrorRecord)args.ItemAdded;
                    Client.SendAsync("WriteErrorLine", record.ToString()).Wait();
                };
                try {
                    ps.Runspace = runspace;
                    if (invokeCommand is DefaultCommand defaultCommand) 
                    {
                        var result = ps.InvokeWithParameters(content, defaultCommand.Parameters);
                        await Client.SendAsync("DefaultResult", JsonConvert.SerializeObject(result));
                    }
                    if (invokeCommand is DetailInvokeCommand detailCmd) 
                    {
                        var result = ps.InvokeWithPipeline(content, detailCmd.InputObject);
                        await Client.SendAsync("DetailResult", JsonConvert.SerializeObject(result));
                    }
                    else if (invokeCommand is ActionInvokeCommand actionCmd) {
                        var result = ps.InvokeWithPipeline(content, actionCmd.InputObject);
                        await Client.SendAsync("ActionResult", actionCmd.ScriptId, JsonConvert.SerializeObject(result));
                    }
                    else if (invokeCommand is GetParametersCommand) {
                        var parameters = ps.GetParameters(content);
                        await Client.SendAsync("GetParameters", parameters.Select(p => new {
                            p.Name,
                            p.Mandatory,
                            p.HelpMessage,
                            p.HelpMessageBaseName,
                            p.ParameterSetName,
                            p.Position,
                            p.ValueFromPipeline,
                            p.ValueFromPipelineByPropertyName,
                            p.ValueFromRemainingArguments,
                            Type = p.Type.FullName
                        }));
                    }
                } 
                catch(ParseException parseException) {
                    await Client.SendAsync("ParseError", parseException.Message);
                }
                catch(Exception exception) {
                    await Client.SendAsync("WriteErrorLine", exception.Message);
                }
            }
        }
        public string CurrentConnectionId;
        public IClientProxy Client => _hub.Clients.Client(CurrentConnectionId);
        private async Task<bool> WaitAndCheckCancel(int milliseconds)
        {
            await Task.Delay(milliseconds);

            if (_context.IsCancel(CurrentConnectionId)) 
            {
                _context.FinalizeCancel(CurrentConnectionId);
                return true;
            }

            return false;
        }
        public Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                Dictionary<string, PSObject> result;
                Client.SendAsync("Prompt", caption, message, descriptions).Wait();
                while (!io.PromptQueue.TryDequeue(out result))
                {
                    if (WaitAndCheckCancel(100).Result)
                        break;
                    
                    if (io.InvokeQueue.Count > 0)
                        throw new InvalidOperationException();
                }
                return result;
            }

            throw new OperationCanceledException();
        }

        public int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                int result;
                Client.SendAsync("PromptForChoice", caption, message, choices, defaultChoice).Wait();
                while (!io.ChoiceQueue.TryDequeue(out result)) 
                {
                    if (WaitAndCheckCancel(100).Result)
                        break;
                    
                    if (io.InvokeQueue.Count > 0)
                        throw new InvalidOperationException();
                }
                return result;
            }

            throw new OperationCanceledException();
        }

        public PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                PSCredential result;
                Client.SendAsync("PromptForCredential", caption, message, userName, targetName).Wait();
                while (!io.CredentialQueue.TryDequeue(out result))
                {
                    if (WaitAndCheckCancel(100).Result)
                        break;

                    if (io.InvokeQueue.Count > 0)
                        throw new InvalidOperationException();
                }
                return result;
            }

            throw new OperationCanceledException();
        }

        public PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                PSCredential result;
                Client.SendAsync("PromptForCredentialWithType", caption, message, userName, targetName, allowedCredentialTypes).Wait();
                while (!io.CredentialQueue.TryDequeue(out result))
                {
                    if (WaitAndCheckCancel(100).Result)
                        break;

                    if (io.InvokeQueue.Count > 0)
                        throw new InvalidOperationException();
                }
                return result;
            }

            throw new OperationCanceledException();
        }

        public string ReadLine()
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                string result;
                Client.SendAsync("ReadLine").Wait();
                while (!io.LineQueue.TryDequeue(out result))
                {
                    if (WaitAndCheckCancel(100).Result)
                        break;
                    
                    if (io.InvokeQueue.Count > 0)
                        throw new InvalidOperationException();
                }
                return result;
            }

            throw new OperationCanceledException();
        }

        public SecureString ReadLineAsSecureString()
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                var result = new SecureString();
                string line;
                Client.SendAsync("ReadLine").Wait();
                while (!io.LineQueue.TryDequeue(out line))
                {
                    if (WaitAndCheckCancel(100).Result)
                        break;
                    
                    if (io.InvokeQueue.Count > 0)
                        throw new InvalidOperationException();
                }
                foreach (var c in line.ToCharArray())
                {
                    result.AppendChar(c);
                }
                return result;
            }

            throw new OperationCanceledException();
        }

        public void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Client.SendAsync("WriteWithColor", foregroundColor, backgroundColor, value).Wait();
        }

        public void Write(string value)
        {
            Client.SendAsync("Write", value).Wait();
        }

        public void WriteDebugLine(string message)
        {
            Client.SendAsync("WriteDebugLine", message).Wait();
        }

        public void WriteErrorLine(string value)
        {
            Client.SendAsync("WriteErrorLine", value).Wait();
        }

        public void WriteLine(string value)
        {
            Client.SendAsync("WriteLine", value).Wait();
        }

        public void WriteProgress(long sourceId, ProgressRecord record)
        {
            Client.SendAsync("WriteProgress", sourceId, record).Wait();
        }

        public void WriteVerboseLine(string message)
        {
            Client.SendAsync("WriteVerboseLine", message).Wait();
        }

        public void WriteWarningLine(string message)
        {
            Client.SendAsync("WriteWarningLine", message).Wait();
        }
    }
}