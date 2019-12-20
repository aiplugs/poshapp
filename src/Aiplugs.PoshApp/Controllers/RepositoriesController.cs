using System.Threading.Tasks;
using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.Services;
using Aiplugs.PoshApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Aiplugs.PoshApp.Controllers
{
    public class RepositoriesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ScriptsService _service;
        private readonly GitContext _gitContext;
        public RepositoriesController(IMapper mapper, ScriptsService service, GitContext gitContext)
        {
            _mapper = mapper;
            _service = service;
            _gitContext = gitContext;
        }
        [HttpGet("/api/repositories")]
        public async Task<IActionResult> Get()
        {
            var repositories =  await _service.GetRepositories();

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
                _gitContext.Invoke(new CloneCommand { ConnectionId = model.ConnectionId, Origin = model.Origin });
            
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
    }
}