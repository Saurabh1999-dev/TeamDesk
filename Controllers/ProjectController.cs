using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IProjectService projectService, ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ProjectResponse>>> GetAllProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, new { message = "An error occurred while retrieving projects" });
            }
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProjectById(Guid id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);

                if (project == null)
                {
                    return NotFound(new { message = $"Project with ID {id} not found" });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving project" });
            }
        }

        /// <summary>
        /// Create new project (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProjectResponse>> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var project = await _projectService.CreateProjectAsync(request);
                return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { message = "An error occurred while creating project" });
            }
        }

        /// <summary>
        /// Update existing project
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ProjectResponse>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var project = await _projectService.UpdateProjectAsync(id, request);
                return Ok(project);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with ID {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while updating project" });
            }
        }

        /// <summary>
        /// Delete project (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteProject(Guid id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Project with ID {id} not found" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting project" });
            }
        }

        /// <summary>
        /// Get projects by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<ProjectResponse>>> GetProjectsByStatus(string status)
        {
            try
            {
                var projects = await _projectService.GetProjectsByStatusAsync(status);
                return Ok(projects);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects by status {Status}", status);
                return StatusCode(500, new { message = "An error occurred while retrieving projects" });
            }
        }

        /// <summary>
        /// Get overdue projects
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<List<ProjectResponse>>> GetOverdueProjects()
        {
            try
            {
                var projects = await _projectService.GetOverdueProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue projects");
                return StatusCode(500, new { message = "An error occurred while retrieving overdue projects" });
            }
        }

        /// <summary>
        /// Assign staff to project
        /// </summary>
        [HttpPost("{id}/staff")]
        [Authorize]
        public async Task<ActionResult<ProjectResponse>> AssignStaffToProject(Guid id, [FromBody] ProjectStaffAssignmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var project = await _projectService.AssignStaffToProjectAsync(id, request);
                return Ok(project);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while assigning staff to project" });
            }
        }

        /// <summary>
        /// Remove staff from project
        /// </summary>
        [HttpDelete("{projectId}/staff/{staffId}")]
        [Authorize]
        public async Task<ActionResult> RemoveStaffFromProject(Guid projectId, Guid staffId)
        {
            try
            {
                var result = await _projectService.RemoveStaffFromProjectAsync(projectId, staffId);

                if (!result)
                {
                    return NotFound(new { message = "Staff assignment not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing staff from project");
                return StatusCode(500, new { message = "An error occurred while removing staff from project" });
            }
        }
    }
}
