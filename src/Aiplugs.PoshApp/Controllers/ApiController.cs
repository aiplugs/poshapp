using System.Linq;
using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.Services;
using Aiplugs.PoshApp.Services.Git;
using Aiplugs.PoshApp.Services.Git.Commands;
using Aiplugs.PoshApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Aiplugs.PoshApp.Controllers
{
    public class ApiController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ScriptsService _service;
        private readonly GitContext _gitContext;
        private readonly LicenseService _license;
        public ApiController(IMapper mapper, ScriptsService service, GitContext gitContext, LicenseService licenseService)
        {
            _mapper = mapper;
            _service = service;
            _gitContext = gitContext;
            _license = licenseService;
        }

        [HttpGet("/api/activation")]
        public async Task<IActionResult> GetActivation()
        {
            var status = (await _license.GetActivationStatus()).ToString();
            var requestCode = _license.GetActivationRequestCode();

            return Json(new { status, requestCode });
        }

        [HttpPost("/api/activation")]
        public async Task<IActionResult> PostActivation([FromBody]ActivationViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var status = (await _license.RegisterActivationCode(model.ActivationCode)).ToString();
            var requestCode = _license.GetActivationRequestCode();

            return Json(new { status, requestCode });
        }

        [HttpPost("/api/reflesh")]
        public async Task<IActionResult> RefleshActivation()
        {
            await _license.Reflesh();
            var status = (await _license.GetActivationStatus()).ToString();
            var requestCode = _license.GetActivationRequestCode();

            return Json(new { status, requestCode });
        }

        [HttpGet("/api/repositories")]
        public async Task<IActionResult> GetRepositories()
        {
            var repositories = await _service.GetRepositories();
            return Json(repositories);
        }

        [HttpPost("/api/repositories")]
        public async Task<IActionResult> PostRepository([FromBody]RepositoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            if (await _service.ExistRepository(model.Name))
                return Conflict();

            var status = await _license.GetActivationStatus();
            if (status == ActivationStatus.None)
            {
                var repositories = await _service.GetRepositories();
                if (repositories.Count() > Limitation.FREE_PLAN_MAX_REPOSITORIES)
                    return new StatusCodeResult(402);
            }

            if (!string.IsNullOrEmpty(model.Origin))
                _gitContext.Invoke(_mapper.Map<CloneCommand>(model));

            await _service.AddRepository(_mapper.Map<Repository>(model));

            return Ok();
        }

        [HttpPost("/api/repositories/{name}")]
        public async Task<IActionResult> PutRepository([FromRoute]string name, [FromBody]RepositoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            if (!await _service.ExistRepository(name))
                return NotFound();

            await _service.AddRepository(_mapper.Map<Repository>(model));

            return Ok();
        }

        [HttpDelete("/api/repositories/{name}")]
        public async Task<IActionResult> DeleteRepository([FromRoute]string name)
        {
            if (!await _service.ExistRepository(name))
                return NotFound();

            await _service.RemoveRepository(name);

            return NoContent();
        }

        [HttpGet("/api/scripts/")]
        public async Task<IActionResult> GetScripts()
        {
            var scripts = await _service.GetScriptList();
            return Json(scripts);
        }

        [HttpGet("/api/repositories/{repositoryName}/scripts/{id}")]
        public async Task<IActionResult> GetScript([FromRoute]string repositoryName, [FromRoute]string id)
        {
            var repository = await _service.GetRepository(repositoryName);

            if (repository == null)
                return NotFound();

            var script = await _service.GetScript(repository, id);
            
            if (script == null)
                return NotFound();

            return Json(script);
        }

        [HttpGet("/api/repositories/{repositoryName}/scripts/{id}/content")]
        public async Task<IActionResult> GetScriptContent([FromRoute]string repositoryName, [FromRoute]string id)
        {
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            var script = await _service.GetScript(repository, id);
            
            if (script == null)
                return NotFound();

            var content = await _service.GetScriptContent(repository, script);

            return Content(content, "text/plain");
        }

        [HttpPut("/api/repositories/{repositoryName}/scripts/{id}/content")]
        public async Task<IActionResult> PutScriptContent([FromRoute]string repositoryName, [FromRoute]string id, [FromBody]string content)
        {
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            var script = await _service.GetScript(repository, id);
            
            if (script == null)
                return NotFound();

            await _service.UpdateScriptContent(repository, script, content);
            
            return NoContent();
        }

        [HttpPost("/api/repositories/{repositoryName}/scripts@list")]
        public async Task<IActionResult> CreateListScript([FromRoute]string repositoryName, [FromBody]ListScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (await _service.ExistScript(repository, model.Id))
                return Conflict();

            var status = await _license.GetActivationStatus();
            if (status == ActivationStatus.None)
            {
                var scripts = await _service.GetScriptList();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return new StatusCodeResult(402);
            }

            await _service.AddScript(repository, _mapper.Map<ListScript>(model));

            return NoContent();
        }

        [HttpPut("/api/repositories/{repositoryName}/scripts@list/{id}")]
        public async Task<IActionResult> UpdateListScript([FromRoute]string repositoryName, [FromRoute]string id, [FromBody]ListScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (!await _service.ExistScript(repository, id))
                return NotFound();

            if (id != model.Id && await _service.ExistScript(repository, model.Id))
                return Conflict();

            await _service.UpdateScript(repository, id, _mapper.Map<ListScript>(model));

            return NoContent();
        }

        [HttpPost("/api/repositories/{repositoryName}/scripts@detail")]
        public async Task<IActionResult> CreateDetailScript([FromRoute]string repositoryName, [FromBody]DetailScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (await _service.ExistScript(repository, model.Id))
                return Conflict();

            var status = await _license.GetActivationStatus();
            if (status == ActivationStatus.None)
            {
                var scripts = await _service.GetScriptList();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return new StatusCodeResult(402);
            }

            await _service.AddScript(repository, _mapper.Map<DetailScript>(model));

            return NoContent();
        }

        [HttpPut("/api/repositories/{repositoryName}/scripts@detail/{id}")]
        public async Task<IActionResult> UpdateDetailScript([FromRoute]string repositoryName, [FromRoute]string id, [FromBody]DetailScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (!await _service.ExistScript(repository, id))
                return NotFound();
            
            if (id != model.Id && await _service.ExistScript(repository, model.Id))
                return Conflict();

            await _service.UpdateScript(repository, id, _mapper.Map<DetailScript>(model));

            return NoContent();
        }

        [HttpPost("/api/repositories/{repositoryName}/scripts@singleton")]
        public async Task<IActionResult> CreateSingletonScript([FromRoute]string repositoryName, [FromBody]SingletonScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (await _service.ExistScript(repository, model.Id))
                return Conflict();

            var status = await _license.GetActivationStatus();
            if (status == ActivationStatus.None)
            {
                var scripts = await _service.GetScriptList();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return new StatusCodeResult(402);
            }

            await _service.AddScript(repository, _mapper.Map<SingletonScript>(model));

            return NoContent();
        }

        [HttpPut("/api/repositories/{repositoryName}/scripts@singleton/{id}")]
        public async Task<IActionResult> UpdateSingletonScript([FromRoute]string repositoryName, [FromRoute]string id, [FromBody]SingletonScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (!await _service.ExistScript(repository, id))
                return NotFound();
            
            if (id != model.Id && await _service.ExistScript(repository, model.Id))
                return Conflict();

            await _service.UpdateScript(repository, id, _mapper.Map<SingletonScript>(model));

            return NoContent();
        }

        [HttpPost("/api/repositories/{repositoryName}/scripts@action")]
        public async Task<IActionResult> CreateActionScript([FromRoute]string repositoryName, [FromBody]ActionScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (await _service.ExistScript(repository, model.Id))
                return Conflict();

            var status = await _license.GetActivationStatus();
            if (status == ActivationStatus.None)
            {
                var scripts = await _service.GetScriptList();
                if (scripts[repository.Name].Count() > Limitation.FREE_PLAN_MAX_SCRIPTS)
                    return new StatusCodeResult(402);
            }

            await _service.AddScript(repository, _mapper.Map<ActionScript>(model));

            return NoContent();
        }

        [HttpPut("/api/repositories/{repositoryName}/scripts@action/{id}")]
        public async Task<IActionResult> UpdateActionScript([FromRoute]string repositoryName, [FromRoute]string id, [FromBody]ActionScriptViewModel model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (!await _service.ExistScript(repository, id))
                return NotFound();

            if (id != model.Id && await _service.ExistScript(repository, model.Id))
                return Conflict();

            await _service.UpdateScript(repository, id, _mapper.Map<ActionScript>(model));

            return NoContent();
        }

        [HttpDelete("/api/repositories/{repositoryName}/scripts/{id}")]
        public async Task<IActionResult> DeleteScript([FromRoute]string repositoryName, [FromRoute]string id)
        {
            var repository = await _service.GetRepository(repositoryName);
            
            if (repository == null)
                return NotFound();

            if (!await _service.ExistScript(repository, id))
                return NotFound();

            await _service.RemoveScript(repository, id);

            return NoContent();
        }
    }
}