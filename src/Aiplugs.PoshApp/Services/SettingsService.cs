using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.Services;

namespace Aiplugs.PoshApp
{
    public class SettingsService
    {
        private readonly IDictionary<ScriptType, string> initialScriptTexts = new Dictionary<ScriptType, string> 
        {
            [ScriptType.List] = "param(\n\t[int]\n\t$Page,\n\n\t[int]\n\t$PageSize\n)\n",
            [ScriptType.Detail] = "param(\n\t[Parameter(ValueFromPipeline=$true,Mandatory=$true)]\n\t[PSObject]\n\t$InputObject\n)\n",
            [ScriptType.Singleton] = "",
            [ScriptType.Action] = "param(\n\t[Parameter(ValueFromPipeline=$true)]\n\t[PSObject[]]\n\t$InputObject\n)\nprocess {\n\n}"
        };
        private readonly ConfigAccessor _configAccessor;
        private readonly RepositoryAccessor _repositoryAccessor;
        public SettingsService(ConfigAccessor configAccessor, RepositoryAccessor repositoryAccessor)
        {
            _configAccessor = configAccessor;
            _repositoryAccessor = repositoryAccessor;
        }
        public async Task<IEnumerable<Script>> GetScriptList()
        {
            var config = await _configAccessor.LoadConfigAsync();

            return config.Scripts;
        }
        public async Task<Script> GetScript(string id)
        {
            var config = await _configAccessor.LoadConfigAsync();

            return config.Scripts.FirstOrDefault(s => s.Id == id);
        }
        public async Task<string> GetScriptContent(Script script)
        {
            return await File.ReadAllTextAsync(script.Path);
        }
        public async Task UpdateScriptContent(Script script, string content)
        {
            await File.WriteAllTextAsync(script.Path, content);
        }
        public async Task<bool> ExistScript(string id)
        {
            return await GetScript(id) != null;
        }
        public async Task AddScript(Script script)
        {
            script.Path = StorageHelper.GetScriptPath(script.Id);

            var config = await _configAccessor.LoadConfigAsync();

            config.Scripts = config.Scripts.Append(script ?? throw new ArgumentNullException(nameof(script)));

            await _configAccessor.SaveConfigAsync(config);

            StorageHelper.CreateScriptsDirIfNotExist();
            await File.WriteAllTextAsync(script.Path, initialScriptTexts[script.Type]);
        }
        public async Task UpdateScript(string id, Script script)
        {
            var config = await _configAccessor.LoadConfigAsync();
            var scripts = config.Scripts.ToArray();
            var index = Array.FindIndex(scripts, 0, scripts.Length, s => s.Id == id);

            script.Path = scripts[index].Path;
            scripts[index] = script;

            if (id != script.Id)
            {
                var src = script.Path;
                var dst = StorageHelper.GetScriptPath(script.Id);
                File.Move(src, dst);
                script.Path = dst;
            }

            config.Scripts = scripts;

            await _configAccessor.SaveConfigAsync(config);
        }
        public async Task RemoveScript(string id)
        {
            var config = await _configAccessor.LoadConfigAsync();
            var scripts = config.Scripts.ToList();
            var script = scripts.First(s => s.Id == id);

            if (script.Type == ScriptType.Detail)
            {
                foreach(var hasDetail in scripts.Where(s => s.Type == ScriptType.List).Cast<ListScript>().Where(s => s.Detail == id)) 
                {
                    hasDetail.Detail = null;
                }
            }
            else if (script.Type == ScriptType.Action)
            {
                foreach(var hasAction in scripts.Where(s => s.Type != ScriptType.Action).Cast<IActionTarget>().Where(s => s.Actions.Contains(id)))
                {
                    hasAction.Actions = hasAction.Actions.Where(actionId => actionId != id).ToArray();
                }
            }

            scripts.Remove(script);
            config.Scripts = scripts;

            await _configAccessor.SaveConfigAsync(config);

            if (File.Exists(script.Path))
            {
                File.Delete(script.Path);
            }
        }
    }

    public class ScriptsService
    {
        private readonly IDictionary<ScriptType, string> initialScriptTexts = new Dictionary<ScriptType, string> 
        {
            [ScriptType.List] = "param(\n\t[int]\n\t$Page,\n\n\t[int]\n\t$PageSize\n)\n",
            [ScriptType.Detail] = "param(\n\t[Parameter(ValueFromPipeline=$true,Mandatory=$true)]\n\t[PSObject]\n\t$InputObject\n)\n",
            [ScriptType.Singleton] = "",
            [ScriptType.Action] = "param(\n\t[Parameter(ValueFromPipeline=$true)]\n\t[PSObject[]]\n\t$InputObject\n)\nprocess {\n\n}"
        };
        private readonly RepositoryAccessor _configAccessor;
        public ScriptsService(RepositoryAccessor configAccessor)
        {
            _configAccessor = configAccessor;
        }
        public async Task<IEnumerable<Repository>> GetRepositories()
        {
            var rootConfig = await _configAccessor.LoadRootConfigAsync();
            return rootConfig.Repositories;
        }
        public async Task<IReadOnlyDictionary<string, IEnumerable<Script>>> GetScriptList()
        {
            var rootConfig = await _configAccessor.LoadRootConfigAsync();
            var result = new Dictionary<string, IEnumerable<Script>>(rootConfig.Repositories.Count());
            foreach (var repo in rootConfig.Repositories)
            {
                var config = await _configAccessor.LoadConfigAsync(repo);
                result.Add(repo.Name, config.Scripts);
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
            var config = await _configAccessor.LoadRootConfigAsync();

            config.Repositories = config.Repositories.Append(repository ?? throw new ArgumentNullException(nameof(repository)));
            
            await _configAccessor.SaveRootConfigAsync(config);
        }
        public async Task RemoveRepository(string repositoryName)
        {
            var config = await _configAccessor.LoadRootConfigAsync();
            var repositories = config.Repositories.ToList();
            var repository = repositories.First(repo => repo.Name == repositoryName);

            repositories.Remove(repository);
            config.Repositories = repositories;

            await _configAccessor.SaveRootConfigAsync(config);
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
            script.Path = GetScriptFileName(script);

            var config = await _configAccessor.LoadConfigAsync(repository);

            config.Scripts = config.Scripts.Append(script ?? throw new ArgumentNullException(nameof(script)));

            await _configAccessor.SaveConfigAsync(repository, config);

            await File.WriteAllTextAsync(GetScriptPath(repository, script.Path), initialScriptTexts[script.Type]);
        }
        public async Task UpdateScript(Repository repository, string id, Script script)
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
        public async Task RemoveScript(Repository repository, string id)
        {
            var config = await _configAccessor.LoadConfigAsync(repository);
            var scripts = config.Scripts.ToList();
            var script = scripts.First(s => s.Id == id);

            if (script.Type == ScriptType.Detail)
            {
                foreach(var hasDetail in scripts.Where(s => s.Type == ScriptType.List).Cast<ListScript>().Where(s => s.Detail == id)) 
                {
                    hasDetail.Detail = null;
                }
            }
            else if (script.Type == ScriptType.Action)
            {
                foreach(var hasAction in scripts.Where(s => s.Type != ScriptType.Action).Cast<IActionTarget>().Where(s => s.Actions.Contains(id)))
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
    }
}