using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {

        private readonly IprojectService _projectService;

        public ProjectController(IprojectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            var success = await _projectService.CreateProjectAsync(dto);
            if (success)
                return Ok(new { message = "Project created successfully." });

            return BadRequest(new { message = "Failed to create project." });
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProjectDto dto)
        {
            var success = await _projectService.UpdateProjectAsync(id, dto);
            if (!success) return NotFound();
            return Ok(new { message = "Project updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Project deleted successfully." });
        }

    }
}
