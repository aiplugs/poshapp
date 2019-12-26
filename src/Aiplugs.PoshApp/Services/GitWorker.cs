using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Exceptions;
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

    public class GitIO
    {
        public readonly ConcurrentQueue<(string username, string password)> CredentialQueue = new ConcurrentQueue<(string username, string password)>();
    }

    public class GitContext
    {
        private readonly ConcurrentQueue<GitCommand> _invokeQueue = new ConcurrentQueue<GitCommand>();
        private readonly ConcurrentDictionary<string, object> _cancelTable = new ConcurrentDictionary<string, object>();
        public readonly ConcurrentDictionary<string, GitIO> IO = new ConcurrentDictionary<string, GitIO>();
        public void Invoke(GitCommand cmd)
        {
            _invokeQueue.Enqueue(cmd);
        }
        public bool TryDequeueCommand(out GitCommand cmd)
        {
            return _invokeQueue.TryDequeue(out cmd);
        }
        public void Start(string connectionId)
        {
            IO.AddOrUpdate(connectionId, key => new GitIO(), (key, current) => new GitIO());
        }
        public bool IsCancel(string connectionId)
        {
            return !string.IsNullOrEmpty(connectionId) && _cancelTable.ContainsKey(connectionId);
        }
        public void Cancel(string connectionId)
        {
            _cancelTable.AddOrUpdate(connectionId, key => new object(), (key, current) => new object());
        }
        public void FinalizeCancel(string connectionId)
        {
            IO.TryRemove(connectionId, out var _);
            _cancelTable.TryRemove(connectionId, out var __);
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

        private string CurrentConnectionId;
        private IClientProxy Client => _hub.Clients.Client(CurrentConnectionId);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_context.TryDequeueCommand(out var command))
                {
                    CurrentConnectionId = command.ConnectionId;
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
                    catch (LibGit2Sharp.RepositoryNotFoundException)
                    {
                        await _hub.Clients.Client(command.ConnectionId).SendAsync("GitLogNotFound", command.Name);
                    }
                    catch (Exception exception)
                    {
                        await _hub.Clients.Client(command.ConnectionId).SendAsync("WriteErrorLine", exception.Message);
                    }
                }
                await Task.Delay(100);
            }
        }
        public LibGit2Sharp.Credentials GetCredential(string url, string usernameFromUrl, LibGit2Sharp.SupportedCredentialTypes types)
        {
            var credential = AdysTech.CredentialManager.CredentialManager.GetCredentials(url);
            if (credential == null)
            {
                var (username, password) = PromptForGitCredential(url, usernameFromUrl);
                credential = new System.Net.NetworkCredential(username, password);
                AdysTech.CredentialManager.CredentialManager.SaveCredentials(url, credential);
            }
            return new LibGit2Sharp.SecureUsernamePasswordCredentials
            {
                Username = credential.UserName,
                Password = credential.SecurePassword
            };
        }
        public void Clone(CloneCommand cmd)
        {
            try
            {
                LibGit2Sharp.Repository.Clone(cmd.Origin, cmd.Path, new LibGit2Sharp.CloneOptions
                {
                    RecurseSubmodules = true,
                    OnProgress = (progress) =>
                    {
                        Client.SendAsync("GitProgress", cmd.Name, progress).Wait();
                        return true;
                    },
                    CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(GetCredential)
                });
            }
            catch(Exception)
            {
                AdysTech.CredentialManager.CredentialManager.RemoveCredentials(cmd.Origin);
                Client.SendAsync("GitCloneFaild", cmd.Name).Wait();
                throw;
            }

            Client.SendAsync("GitClone", cmd.Name).Wait();
        }

        public void Fetch(FetchCommand cmd)
        {   
            using var repository = new LibGit2Sharp.Repository(cmd.Path);
            var remote = repository.Network.Remotes["origin"];
            if (remote != null) {
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                var logMessage = "";
                LibGit2Sharp.Commands.Fetch(repository, remote.Name, refSpecs, new LibGit2Sharp.FetchOptions
                {
                    OnProgress = progress =>
                    {
                        Client.SendAsync("GitFetchProgress", cmd.Name, progress).Wait();
                        return true;
                    },
                    CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(GetCredential)
                }, logMessage);
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

            var filter = new LibGit2Sharp.CommitFilter { IncludeReachableFrom = repository.Refs };

            var top = repository.Commits.QueryBy(filter).Take(100).Select(o => new {
                Commit = o.Id.Sha,
                Message = o.Message,
                MessageShort = o.MessageShort,
                Name = o.Author.Name,
                Email = o.Author.Email,
                When = o.Author.When
            });

            var remote = repository.Branches["origin/master"]?.Tip.Id.Sha;
            var local = repository.Head.Tip.Id.Sha;

            Client.SendAsync("GitLog", cmd.Name, top, remote, local);

            var scripts = _scriptsService.GetScriptList().Result;
            var status = repository.RetrieveStatus(new LibGit2Sharp.StatusOptions()).Select(item => new {
                State = item.State.ToString(),
                Script = scripts[cmd.Name].FirstOrDefault(script => script.Path == item.FilePath)?.Id
            });
            Client.SendAsync("GitStatus", cmd.Name, status);
        }

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

        private TResult IOWork<TResult>(Func<GitIO, TResult> action)
        {
            if (_context.IO.TryGetValue(CurrentConnectionId, out var io))
            {
                return action(io);
            }

            throw new InvalidOperationException($"Cannot found connectionId ({CurrentConnectionId})");
        }

        public (string username, string password) PromptForGitCredential(string url, string usernameFromUrl)
        {
            return IOWork(io =>
            {
                Client.SendAsync("PromptForGitCredential", url, usernameFromUrl).Wait();
                while (true)
                {
                    if (io.CredentialQueue.TryDequeue(out var result))
                        return result;

                    if (WaitAndCheckCancel(100).Result)
                        throw new PoshAppCanceledException("Prompt for git credential is canceled");
                }
            });
        }
    }
}