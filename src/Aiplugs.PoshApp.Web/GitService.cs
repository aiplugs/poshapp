using Aiplugs.PoshApp.Services;
using Aiplugs.PoshApp.Web.Models;
using LibGit2Sharp;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Web
{
    public class GitService
    {
        private readonly JsonRpc _rpc;
        private readonly ICredentialManager _credentialManager;
        private readonly string _dataPath;
        string ResolvePath(string name) => Path.Combine(_dataPath, name);
        public GitService(IJsonRpcMessageHandler handler, ICredentialManager credentialManager, string dataDir = null)
        {
            _rpc = new JsonRpc(handler, this);
            _rpc.CancelLocallyInvokedMethodsWhenConnectionIsClosed = true;
            _rpc.StartListening();

            _credentialManager = credentialManager;

            _dataPath = new ConfigAccessor(dataDir).AppPath;
        }
        internal async Task StartAsync()
        {
            await _rpc.Completion;
        }

        public IEnumerable<StatusReponse> GetStatus(string name)
        {
            IEnumerable<string> labels(FileStatus state)
            {
                if (state.HasFlag(FileStatus.NewInWorkdir))
                    yield return "NEW";
                if (state.HasFlag(FileStatus.NewInIndex))
                    yield return "NEW";
                if (state.HasFlag(FileStatus.ModifiedInWorkdir))
                    yield return "MODIFIED";
                if (state.HasFlag(FileStatus.ModifiedInIndex))
                    yield return "MODIFIED";
                if (state.HasFlag(FileStatus.TypeChangeInWorkdir))
                    yield return "TYPECHANGE";
                if (state.HasFlag(FileStatus.TypeChangeInIndex))
                    yield return "TYPECHANGE";
                if (state.HasFlag(FileStatus.RenamedInWorkdir))
                    yield return "RENAMED";
                if (state.HasFlag(FileStatus.RenamedInIndex))
                    yield return "RENAMED";
                if (state.HasFlag(FileStatus.Ignored))
                    yield return "IGNORED";
            }

            using var repository = new Repository(ResolvePath(name));

            return repository.RetrieveStatus(new StatusOptions()).Select(item => new StatusReponse {
                File = item.FilePath,
                Labels = labels(item.State)
            });
        }
        public LogResponse GetLog(string name)
        {
            using var repository = new Repository(ResolvePath(name));

            var filter = new CommitFilter { IncludeReachableFrom = repository.Refs };

            return new LogResponse
            {
                Logs = repository.Commits.QueryBy(filter).Take(100).Select(o => new Models.LogEntry
                {
                    Commit = o.Id.Sha,
                    Name = o.Author.Name,
                    Email = o.Author.Email,
                    When = o.Author.When,
                    Message = o.Message,
                    MessageShort = o.MessageShort,
                }).ToArray(),
                Origin = repository.Branches["origin/master"]?.Tip?.Id.Sha,
                Local = repository.Head?.Tip?.Id.Sha
            };
        }

        public HttpStatusCode Clone(string origin, string name)
        {
            var path = ResolvePath(name);

            if (Repository.IsValid(path))
                return HttpStatusCode.Conflict;

            string credentialDomain = null;
            NetworkCredential credential = null;
            Repository.Clone(origin, path, new CloneOptions
            {
                RecurseSubmodules = true,
                CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler((url, username, type) =>
                {
                    credential = _credentialManager.GetCredentials(url);
                    if (credential == null)
                    {
                        using var context = new JoinableTaskContext();
                        var jtf = new JoinableTaskFactory(context);
                        var c = jtf.Run(() => _rpc.InvokeAsync<Credential>("PromptForGitCredential", url, username));

                        credential = new NetworkCredential(c.Username, c.Password);
                        credentialDomain = url;
                    }

                    return new SecureUsernamePasswordCredentials
                    {
                        Username = credential.UserName,
                        Password = credential.SecurePassword
                    };
                })
            });

            if (credentialDomain != null && credential != null)
            {
                _credentialManager.SaveCredentials(credentialDomain, credential);
            }

            return HttpStatusCode.OK;
        }

        public HttpStatusCode Fetch(string name)
        {
            using var repository = new Repository(ResolvePath(name));

            var remote = repository.Network.Remotes["origin"];

            if (remote != null)
            {
                string credentialDomain = null;
                NetworkCredential credential = null;
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

                Commands.Fetch(repository, remote.Name, refSpecs, new FetchOptions
                {
                    CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler((url, username, type) =>
                    {
                        credential = _credentialManager.GetCredentials(url);
                        if (credential == null)
                        {
                            using var context = new JoinableTaskContext();
                            var jtf = new JoinableTaskFactory(context);
                            var c = jtf.Run(() => _rpc.InvokeAsync<Credential>("PromptForGitCredential", url, username));

                            credential = new NetworkCredential(c.Username, c.Password);
                            credentialDomain = url;
                        }

                        return new SecureUsernamePasswordCredentials
                        {
                            Username = credential.UserName,
                            Password = credential.SecurePassword
                        };
                    }),
                    OnProgress = progress =>
                    {
                        _rpc.NotifyAsync("GitProgress", progress).ConfigureAwait(false);
                        return true;
                    }
                }, string.Empty);
                

                if (credentialDomain != null && credential != null)
                {
                    _credentialManager.SaveCredentials(credentialDomain, credential);
                }
            }

            return HttpStatusCode.OK;
        }

        public HttpStatusCode Reset(string name)
        {
            using var repository = new Repository(ResolvePath(name));

            var origin = repository.Branches["origin/master"].Tip;

            if (origin != null)
            {
                repository.Reset(ResetMode.Hard, origin);
            }

            return HttpStatusCode.OK;
        }
    }
}
