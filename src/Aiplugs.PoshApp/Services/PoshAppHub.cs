using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Aiplugs.PoshApp.Services
{
    public class PoshAppHub : Hub
    {
        private readonly PowershellContext _powershellContext;
        private readonly GitContext _gitContext;
        private readonly ScriptsService _scriptsService;
        public PoshAppHub(PowershellContext powershellContext, GitContext gitContext, ScriptsService scriptsService)
        {
            _powershellContext = powershellContext;
            _gitContext = gitContext;
            _scriptsService = scriptsService;
        }

        public void Prompt(IDictionary<string, string> fields)
        {

            if (_powershellContext.IO.TryGetValue(Context.ConnectionId, out var io))
            {

                io.PromptQueue.Enqueue(fields.ToDictionary(o => o.Key, o => o.Value != null ? new PSObject(PSSerializer.Deserialize(o.Value)) : null));
            }
        }

        public void PromptForChoice(int choice)
        {
            if (_powershellContext.IO.TryGetValue(Context.ConnectionId, out var io))
            {
                io.ChoiceQueue.Enqueue(choice);
            }
        }

        public void PromptForCredential(string username, string password)
        {
            if (_powershellContext.IO.TryGetValue(Context.ConnectionId, out var io))
            {
                var secure = new SecureString();
                foreach (var c in password.ToCharArray())
                {
                    secure.AppendChar(c);
                }
                var credentail = new PSCredential(username, secure);
                io.CredentialQueue.Enqueue(credentail);
            }
        }

        public void WriteLine(string message)
        {
            if (_powershellContext.IO.TryGetValue(Context.ConnectionId, out var io))
            {
                io.LineQueue.Enqueue(message);
            }
        }

        public void Invoke(string scriptId, Dictionary<string, string> parameters)
        {
            _powershellContext.Invoke(new DefaultCommand
            { 
                ConnectionId = Context.ConnectionId, 
                ScriptId = scriptId,
                Parameters = parameters?.ToDictionary(p => p.Key, p => PSSerializer.Deserialize(p.Value))
            });
        }

        public void InvokeDetail(string scriptId, string clixml)
        {
            _powershellContext.Invoke(new DetailInvokeCommand 
            { 
                ConnectionId = Context.ConnectionId, 
                ScriptId = scriptId, 
                InputObject = PSSerializer.Deserialize(clixml)
            });
        }

        public void InvokeAction(string scriptId, string[] clixmls)
        {
            _powershellContext.Invoke(new ActionInvokeCommand 
            { 
                ConnectionId = Context.ConnectionId, 
                ScriptId = scriptId, 
                InputObject = clixmls?.Select(clixml => PSSerializer.Deserialize(clixml)).ToArray() 
            });
        }

        public void GetParameters(string scriptId)
        {
            _powershellContext.Invoke(new GetParametersCommand { ConnectionId = Context.ConnectionId, ScriptId = scriptId });
        }

        public async Task GitClone(string name, string origin)
        {
            var repository = await _scriptsService.GetRepository(name);
            if (repository != null) 
            {
                _gitContext.Invoke(new CloneCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Path = repository.Path,
                    Origin = origin
                });
            }
        }

        public async Task GitLog(string name)
        {
            var repository = await _scriptsService.GetRepository(name);
            if (repository != null) 
            {
                _gitContext.Invoke(new FetchCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
                _gitContext.Invoke(new LogCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
            }
        }

        public async Task GitForcePull(string name)
        {
            var repository = await _scriptsService.GetRepository(name);
            if (repository != null) 
            {
                _gitContext.Invoke(new FetchCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
                _gitContext.Invoke(new ResetCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
                _gitContext.Invoke(new LogCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
            }
        }

        public async Task GitReset(string name)
        {
            var repository = await _scriptsService.GetRepository(name);
            if (repository != null) 
            {
                _gitContext.Invoke(new ResetCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
                _gitContext.Invoke(new LogCommand
                {
                    ConnectionId = Context.ConnectionId,
                    Name = repository.Name,
                    Path = repository.Path
                });
            }
        }

        public override Task OnConnectedAsync()
        {
            _powershellContext.Start(Context.ConnectionId);
            return Task.FromResult(0);
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _powershellContext.Cancel(Context.ConnectionId);
            return Task.FromResult(0);
        }
    }
}