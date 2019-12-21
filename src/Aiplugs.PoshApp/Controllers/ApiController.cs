using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.Services;
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
        public ApiController(IMapper mapper, ScriptsService service, GitContext gitContext)
        {
            _mapper = mapper;
            _service = service;
            _gitContext = gitContext;
        }

        [HttpGet("/api/repositories")]
        public async Task<IActionResult> Get()
        {
            var repositories = await _service.GetRepositories();

            return Json(repositories);
        }

        [HttpPost("/api/repositories")]
        public async Task<IActionResult> Post([FromBody]RepositoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            if (await _service.ExistRepository(model.Name))
                return Conflict();

            if (!string.IsNullOrEmpty(model.Origin))
                _gitContext.Invoke(_mapper.Map<CloneCommand>(model));

            await _service.AddRepository(_mapper.Map<Repository>(model));

            return Ok();
        }

        [HttpPost("/api/repositories/{name}")]
        public async Task<IActionResult> Put([FromRoute]string name, [FromBody]RepositoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            if (!await _service.ExistRepository(name))
                return NotFound();

            await _service.AddRepository(_mapper.Map<Repository>(model));

            return Ok();
        }

        [HttpDelete("/api/repositories/{name}")]
        public async Task<IActionResult> Delete([FromRoute]string name)
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