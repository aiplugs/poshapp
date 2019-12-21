using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Aiplugs.PoshApp.Services
{
    public abstract class GitCommand 
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class CloneCommand : GitCommand
    {
        public string Origin { get; set; }
    }
    public class FetchCommand : GitCommand
    {
    }
    public class ResetCommand : GitCommand
    {
    }
    public class LogCommand : GitCommand
    {
    }

    public class GitContext
    {
        private readonly ConcurrentQueue<GitCommand> _invokeQueue = new ConcurrentQueue<GitCommand>();
        public void Invoke(GitCommand cmd)
        {
            _invokeQueue.Enqueue(cmd);
        }
        public bool TryDequeueCommand(out GitCommand cmd)
        {
            return _invokeQueue.TryDequeue(out cmd);
        }

    }
    public class GitWorker : BackgroundService
    {
        private readonly IHubContext<PoshAppHub> _hub;
        private readonly GitContext _context;
        private readonly ScriptsService _scriptsService;
        public GitWorker(IHubContext<PoshAppHub> hub, GitContext context, ScriptsService scriptsService)
        {
            _hub = hub;
            _context = context;
            _scriptsService = scriptsService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_context.TryDequeueCommand(out var command))
                {
                    try
                    {
                        if (command is CloneCommand cloneCmd)
                            Clone(cloneCmd);

                        else if (command is FetchCommand fechCmd)
                            Fetch(fechCmd);

                        else if (command is LogCommand logCmd)
                            Log(logCmd);

                        else if (command is ResetCommand resetCmd)
                            Reset(resetCmd);
                    }
                    catch (Exception exception)
                    {
                        await _hub.Clients.Client(command.ConnectionId).SendAsync("WriteErrorLine", exception.Message);
                    }
                }
                await Task.Delay(100);
            }
        }
        public void Clone(CloneCommand cmd)
        {
            LibGit2Sharp.Repository.Clone(cmd.Origin, cmd.Path, new LibGit2Sharp.CloneOptions
            {
                RecurseSubmodules = true,
                OnProgress = (progress) => {
                    _hub.Clients.Client(cmd.ConnectionId).SendAsync("GitProgress", cmd.Name, progress).Wait();
                    return true;
                }
            });
            _hub.Clients.Client(cmd.ConnectionId).SendAsync("GitClone", cmd.Name).Wait();
        }

        public void Fetch(FetchCommand cmd)
        {
            using var repository = new LibGit2Sharp.Repository(cmd.Path);
            var remote = repository.Network.Remotes["origin"];
            if (remote != null) {
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                var opts = new LibGit2Sharp.FetchOptions();
                opts.OnProgress += progress => {
                    _hub.Clients.Client(cmd.ConnectionId).SendAsync("GitFetchProgress", cmd.Name, progress).Wait();
                    return true;
                };
                // opts.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler((url, usernameFromUrl, types) => 
                //     new LibGit2Sharp.UsernamePasswordCredentials() 
                //     {
                //         Username = "USERNAME",
                //         Password = "PASSWORD"
                //     });
                var logMessage = "";
                LibGit2Sharp.Commands.Fetch(repository, remote.Name, refSpecs, opts, logMessage);
            }
        }

        public void Reset(ResetCommand cmd)
        {
            using var repository = new LibGit2Sharp.Repository(cmd.Path);
            
            var origin = repository.Branches["origin/master"].Tip;

            if (origin != null) {
                repository.Reset(LibGit2Sharp.ResetMode.Hard, origin);
            }
        }

        public void Log(LogCommand cmd)
        {
            using var repository = new LibGit2Sharp.Repository(cmd.Path);

            var origin = repository.Branches["origin/master"];

            var commits = origin?.Commits ?? repository?.Commits;

            if (commits == null) {
                _hub.Clients.Client(cmd.ConnectionId).SendAsync("GitLogNotFound", cmd.Name);
                return;
            }
            
            var top100 = commits.Take(100).Select(o => new {
                Commit = o.Id.Sha,
                Message = o.Message,
                MessageShort = o.MessageShort,
                Name = o.Author.Name,
                Email = o.Author.Email,
                When = o.Author.When
            });

            var remote = origin?.Tip.Id.Sha;
            var local = repository.Head.Tip.Id.Sha;

            _hub.Clients.Client(cmd.ConnectionId).SendAsync("GitLog", cmd.Name, top100, remote, local);

            var scripts = _scriptsService.GetScriptList().Result;
            var status = repository.RetrieveStatus(new LibGit2Sharp.StatusOptions()).Select(item => new {
                State = item.State.ToString(),
                Script = scripts[cmd.Name].FirstOrDefault(script => script.Path == item.FilePath)?.Id
            });
            _hub.Clients.Client(cmd.ConnectionId).SendAsync("GitStatus", cmd.Name, status);
        }
    }
}