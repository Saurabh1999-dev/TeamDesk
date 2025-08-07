using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.DTOs.Response;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tasks
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TaskResponse>>> GetAllTasks()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponse>> GetTaskById(Guid id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound(new { message = $"Task with ID {id} not found" });
                }
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving task" });
            }
        }

        /// <summary>
        /// Get tasks by project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<TaskResponse>>> GetTasksByProject(Guid projectId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByProjectAsync(projectId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
            }
        }

        /// <summary>
        /// Get tasks assigned to a staff member
        /// </summary>
        [HttpGet("assigned/{staffId}")]
        public async Task<ActionResult<List<TaskResponse>>> GetTasksByAssignee(Guid staffId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByAssigneeAsync(staffId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for staff {StaffId}", staffId);
                return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
            }
        }

        /// <summary>
        /// Create new task
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User ID not found in token claims" });
                }

                if (!Guid.TryParse(userIdClaim.Value, out var createdById))
                {
                    return BadRequest(new { message = "Invalid user ID format in token" });
                }

                var task = await _taskService.CreateTaskAsync(request, createdById);

                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { message = "An error occurred while creating task" });
            }
        }


        /// <summary>
        /// Update existing task
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<TaskResponse>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = await _taskService.UpdateTaskAsync(id, request);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while updating task" });
            }
        }

        /// <summary>
        /// Delete task (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTask(Guid id)
        {
            try
            {
                var result = await _taskService.DeleteTaskAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Task with ID {id} not found" });
                }
                return Ok(new { success = true, message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting task" });
            }
        }

        /// <summary>
        /// Assign task to staff member
        /// </summary>
        [HttpPost("assign")]
        [Authorize]
        public async Task<ActionResult<TaskResponse>> AssignTask([FromBody] AssignTaskRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = await _taskService.AssignTaskAsync(request.TaskId, request.AssignedToId, request.SendNotification);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task");
                return StatusCode(500, new { message = "An error occurred while assigning task" });
            }
        }

        /// <summary>
        /// Update task status
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<ActionResult<TaskResponse>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = await _taskService.UpdateTaskStatusAsync(id, request.Status);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status with ID {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while updating task status" });
            }
        }

        /// <summary>
        /// Update task progress
        /// </summary>
        [HttpPut("{id}/progress")]
        [Authorize]
        public async Task<ActionResult<TaskResponse>> UpdateTaskProgress(Guid id, [FromBody] UpdateTaskProgressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var task = await _taskService.UpdateTaskProgressAsync(id, request.Progress);
                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task progress with ID {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while updating task progress" });
            }
        }

        /// <summary>
        /// Add comment to task
        /// </summary>
        [HttpPost("{id}/comments")]
        [Authorize]
        public async Task<ActionResult<TaskCommentResponse>> AddTaskComment(Guid id, [FromBody] AddTaskCommentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var comment = await _taskService.AddTaskCommentAsync(id, request.Comment);
                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to task with ID {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while adding comment" });
            }
        }

        /// <summary>
        /// Get task statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<TaskStatsResponse>> GetTaskStats()
        {
            try
            {
                var stats = await _taskService.GetTaskStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving task statistics" });
            }
        }

        [HttpPost("{taskId}/attachments")]
        [Authorize]
        public async Task<ActionResult<TaskAttachmentResponse>> UploadTaskAttachment(Guid taskId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided" });
                }

                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "User ID not found in token claims" });
                }

                var attachment = await _taskService.UploadTaskAttachmentAsync(taskId, file, userId);
                return Ok(attachment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while uploading attachment" });
            }
        }

        /// <summary>
        /// Delete task attachment
        /// </summary>
        [HttpDelete("{taskId}/attachments/{attachmentId}")]
        [Authorize]
        public async Task<ActionResult> DeleteTaskAttachment(Guid taskId, Guid attachmentId)
        {
            try
            {
                var result = await _taskService.DeleteTaskAttachmentAsync(taskId, attachmentId);
                if (!result)
                {
                    return NotFound(new { message = "Attachment not found" });
                }
                return Ok(new { success = true, message = "Attachment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for task {TaskId}", attachmentId, taskId);
                return StatusCode(500, new { message = "An error occurred while deleting attachment" });
            }
        }

        /// <summary>
        /// Get task attachments
        /// </summary>
        [HttpGet("{taskId}/attachments")]
        public async Task<ActionResult<List<TaskAttachmentResponse>>> GetTaskAttachments(Guid taskId)
        {
            try
            {
                var attachments = await _taskService.GetTaskAttachmentsAsync(taskId);
                return Ok(attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while retrieving attachments" });
            }
        }
    }
}
