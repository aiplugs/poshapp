using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.Services;

namespace Aiplugs.PoshApp
{
    public class ScriptsService
    {
        private readonly IDictionary<ScriptType, string> initialScriptTexts = new Dictionary<ScriptType, string> 
        {
            [ScriptType.List] = "param(\n\t[int]\n\t$Page,\n\n\t[int]\n\t$PageSize\n)\n",
            [ScriptType.Detail] = "param(\n\t[Parameter(ValueFromPipeline=$true,Mandatory=$true)]\n\t[PSObject]\n\t$InputObject\n)\n",
            [ScriptType.Singleton] = "",
            [ScriptType.Action] = "param(\n\t[Parameter(ValueFromPipeline=$true)]\n\t[PSObject[]]\n\t$InputObject\n)\nprocess {\n\n}"
        };
        private readonly ConfigAccessor _configAccessor;
        private readonly LicenseService _license;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public ScriptsService(ConfigAccessor configAccessor, LicenseService licenseService)
        {
            _configAccessor = configAccessor;
            _license = licenseService;
        }
        public async Task<IEnumerable<Repository>> GetRepositories()
        {
            var status = await _license.GetActivationStatus();
            var rootConfig = await _configAccessor.LoadRootConfigAsync();
            var repositories = status == ActivationStatus.Valid
                                        ? rootConfig.Repositories
                                        : rootConfig.Repositories.Take(RootConfig.FREE_PLAN_MAX_REPOSITORIES);

            return repositories;
        }
        public async Task<IReadOnlyDictionary<string, IEnumerable<Script>>> GetScriptList()
        {
            var result = new Dictionary<string, IEnumerable<Script>>();
            var status = await _license.GetActivationStatus();
            var rootConfig = await _configAccessor.LoadRootConfigAsync();
            var respositories = status == ActivationStatus.Valid
                                        ? rootConfig.Repositories
                                        : rootConfig.Repositories.Take(RootConfig.FREE_PLAN_MAX_REPOSITORIES);
            foreach (var repo in respositories)
            {
                var config = await _configAccessor.LoadConfigAsync(repo);
                var scripts = status == ActivationStatus.Valid 
                                        ? config.Scripts 
                                        : config.Scripts.Take(RootConfig.FREE_PLAN_MAX_SCRIPTS);
                
                result.Add(repo.Name, scripts);
            }
            return result;
        }
        public async Task<Repository> GetRepository(string repositoryName)
        {
            var config = await _configAccessor.LoadRootConfigAsync();

            return config.Repositories.FirstOrDefault(repo => repo.Name == repositoryName);
        }
        public async Task<bool> ExistRepository(string repositoryName)
        {
            return await GetRepository(repositoryName) != null;
        }
        public async Task AddRepository(Repository repository)
        {
            await _semaphore.WaitAsync();
            try
            {
                var config = await _configAccessor.LoadRootConfigAsync();

                config.Repositories = config.Repositories.Append(repository ?? throw new ArgumentNullException(nameof(repository)));

                await _configAccessor.SaveRootConfigAsync(config);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task RemoveRepository(string repositoryName)
        {
            await _semaphore.WaitAsync();
            try
            {
                var config = await _configAccessor.LoadRootConfigAsync();
                var repositories = config.Repositories.ToList();
                var repository = repositories.First(repo => repo.Name == repositoryName);

                repositories.Remove(repository);
                config.Repositories = repositories;

                await _configAccessor.SaveRootConfigAsync(config);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<Script> GetScript(string scriptId)
        {
            var splited = scriptId.Split(':');
            var repositoryName = splited[0];
            var scriptName = splited[1];

            var repository = await GetRepository(repositoryName);

            if (repository == null)
                return null;

            var config = await _configAccessor.LoadConfigAsync(repository);

            return config.Scripts.FirstOrDefault(s => s.Id == scriptName);
        }
        public async Task<Script> GetScript(Repository repository, string scriptId)
        {
            var config = await _configAccessor.LoadConfigAsync(repository);

            return config.Scripts.FirstOrDefault(s => s.Id == scriptId);
        }
        public async Task<string> GetScriptContent(string scriptId)
        {
            var splited = scriptId.Split(':');
            
            if (splited.Length != 2)
                throw new ArgumentException(nameof(scriptId));
            
            var repositoryName = splited[0];
            
            var scriptName = splited[1];

            var repository = await GetRepository(repositoryName);

            if (repository == null)
                return null;

            var script = await GetScript(repository, scriptName);

            if (script == null)
                return null;

            return await GetScriptContent(repository, script);
        }

        public async Task<string> GetScriptContent(Repository repository, Script script)
        {
            var path = GetScriptPath(repository, script.Path);

            return await File.ReadAllTextAsync(path);
        }
        public async Task UpdateScriptContent(Repository repository, Script script, string content)
        {
            var path = GetScriptPath(repository, script.Path);
            await File.WriteAllTextAsync(path, content);
        }
        public async Task<bool> ExistScript(Repository repository, string scriptId)
        {
            return await GetScript(repository, scriptId) != null;
        }
        private string GetScriptPath(Repository repository, string filename) => Path.Combine(repository.Path, filename);
        private string GetScriptFileName(Script script) => $"{script.Id}.ps1";
        public async Task AddScript(Repository repository, Script script)
        {
            await _semaphore.WaitAsync();
            try
            {
                script.Path = GetScriptFileName(script);

                var config = await _configAccessor.LoadConfigAsync(repository);

                config.Scripts = config.Scripts.Append(script ?? throw new ArgumentNullException(nameof(script)));

                await _configAccessor.SaveConfigAsync(repository, config);

                await File.WriteAllTextAsync(GetScriptPath(repository, script.Path), initialScriptTexts[script.Type]);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task UpdateScript(Repository repository, string id, Script script)
        {
            await _semaphore.WaitAsync();
            try
            {
                var config = await _configAccessor.LoadConfigAsync(repository);
                var scripts = config.Scripts.ToArray();
                var index = Array.FindIndex(scripts, 0, scripts.Length, s => s.Id == id);

                script.Path = scripts[index].Path;
                scripts[index] = script;

                if (id != script.Id)
                {
                    var src = script.Path;
                    var dst = GetScriptFileName(script);
                    File.Move(GetScriptPath(repository, src), GetScriptPath(repository, dst));
                    script.Path = dst;
                }

                config.Scripts = scripts;

                await _configAccessor.SaveConfigAsync(repository, config);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task RemoveScript(Repository repository, string id)
        {
            await _semaphore.WaitAsync();
            try
            {
                var config = await _configAccessor.LoadConfigAsync(repository);
                var scripts = config.Scripts.ToList();
                var script = scripts.First(s => s.Id == id);

                if (script.Type == ScriptType.Detail)
                {
                    foreach (var hasDetail in scripts.Where(s => s.Type == ScriptType.List).Cast<ListScript>().Where(s => s.Detail == id))
                    {
                        hasDetail.Detail = null;
                    }
                }
                else if (script.Type == ScriptType.Action)
                {
                    foreach (var hasAction in scripts.Where(s => s.Type != ScriptType.Action).Cast<IActionTarget>().Where(s => s.Actions.Contains(id)))
                    {
                        hasAction.Actions = hasAction.Actions.Where(actionId => actionId != id).ToArray();
                    }
                }

                scripts.Remove(script);
                config.Scripts = scripts;

                await _configAccessor.SaveConfigAsync(repository, config);

                if (File.Exists(script.Path))
                {
                    File.Delete(script.Path);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}