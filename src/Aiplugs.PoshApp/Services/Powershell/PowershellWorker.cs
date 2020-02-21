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
using Aiplugs.PoshApp.Exceptions;
using Aiplugs.PoshApp.Services.Powersehll.Commands;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerShell;
using Newtonsoft.Json;

namespace Aiplugs.PoshApp.Services.Powersehll
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
                iss.ExecutionPolicy = ExecutionPolicy.Unrestricted;
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

                using var ps = PowerShell.Create(runspace);
                ps.Streams.Error.DataAdding += (sender, args) =>
                {
                    var record = (ErrorRecord)args.ItemAdded;
                    Client.SendAsync("WriteErrorLine", record.ToString()).Wait();
                };
                try
                {
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
                    else if (invokeCommand is ActionInvokeCommand actionCmd) 
                    {
                        var result = ps.InvokeWithPipeline(content, actionCmd.InputObject);
                        await Client.SendAsync("ActionResult", actionCmd.ScriptId, JsonConvert.SerializeObject(result));
                    }
                    else if (invokeCommand is GetParametersCommand) 
                    {
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
                            p.ValidateSet,
                            Type = p.Type.FullName
                        }));
                    }
                } 
                catch(ParseException parseException) 
                {
                    await Client.SendAsync("ParseError", parseException.Message);
                    await Client.SendAsync("UnitResult");
                }
                catch(Exception exception) 
                {
                    await Client.SendAsync("WriteErrorLine", exception.Message);
                    await Client.SendAsync("UnitResult");
                }
            }
        }

        private string CurrentConnectionId;
        private IClientProxy Client => _hub.Clients.Client(CurrentConnectionId);

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

        private TResult IOWork<TResult>(Func<PowershellIO,TResult> action)
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                return action(io);
            }

            throw new InvalidOperationException($"Cannot found connectionId ({CurrentConnectionId})");
        }

        public Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return IOWork(io =>
            {
                Client.SendAsync("Prompt", caption, message, descriptions).Wait();
                while (true)
                {
                    if (io.PromptQueue.TryDequeue(out var result))
                        return result;

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Prompt is canceled");
                }
            });
        }

        public int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return IOWork(io =>
            {
                Client.SendAsync("PromptForChoice", caption, message, choices, defaultChoice).Wait();
                while (true)
                {
                    if (io.ChoiceQueue.TryDequeue(out var result))
                        return result;

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Prompt for choice is canceled");
                }
            });
        }

        public PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return IOWork(io =>
            {
                Client.SendAsync("PromptForCredential", caption, message, userName, targetName).Wait();
                while (true)
                {
                    if (io.CredentialQueue.TryDequeue(out var result))
                        return result;

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Prompt for credential is canceled");
                }
            });
        }

        public PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return IOWork(io =>
            {
                Client.SendAsync("PromptForCredentialWithType", caption, message, userName, targetName, allowedCredentialTypes).Wait();
                while (true)
                {
                    if (io.CredentialQueue.TryDequeue(out var result))
                        return result;

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Prompt for credential is canceled");
                }
            });
        }

        public string ReadLine()
        {
            return IOWork(io =>
            {
                Client.SendAsync("ReadLine").Wait();
                while (true)
                {
                    if (io.LineQueue.TryDequeue(out var result))
                        return result;

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Read line is canceled");
                }
            });
        }

        public SecureString ReadLineAsSecureString()
        {
            return IOWork(io =>
            {
                Client.SendAsync("ReadLine").Wait();
                while (true)
                {
                    if (io.LineQueue.TryDequeue(out var line))
                    {
                        var result = new SecureString();
                        foreach (var c in line.ToCharArray())
                        {
                            result.AppendChar(c);
                        }
                        return result;
                    }

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Read line as secure string is canceled");
                } 
            });
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