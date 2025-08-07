// Controllers/TaskCommentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TeamDesk.DTOs;
using TeamDesk.Hubs;
using TeamDesk.Services.Interfaces;
using System.Security.Claims;
using TeamDesk.DTOs.Request;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskCommentsController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<TaskCommentsController> _logger;

        public TaskCommentsController(
            ITaskService taskService,
            IHubContext<NotificationHub> hubContext,
            ILogger<TaskCommentsController> logger)
        {
            _taskService = taskService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Add comment to task with real-time broadcast
        /// </summary>
        [HttpPost("{taskId}/comments")]
        public async Task<ActionResult<TaskCommentResponse>> AddTaskComment(
            Guid taskId,
            [FromBody] AddTaskCommentRequest request)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "User ID not found in token claims" });
                }

                var comment = await _taskService.AddTaskCommentAsync(taskId, request.Comment, userId);
                await _hubContext.Clients.Group($"task_{taskId}")
                    .SendAsync("NewTaskComment", comment);

                _logger.LogInformation("Added comment to task {TaskId} by user {UserId}", taskId, userId);

                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while adding comment" });
            }
        }

        [HttpGet("{taskId}/comments")]
        public async Task<ActionResult<List<TaskCommentResponse>>> GetTaskComments(Guid taskId)
        {
            try
            {
                var comments = await _taskService.GetTaskCommentsAsync(taskId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while retrieving comments" });
            }
        }

        [HttpDelete("{taskId}/comments/{commentId}")]
        public async Task<ActionResult> DeleteTaskComment(Guid taskId, Guid commentId)
        {
            try
            {
                var result = await _taskService.DeleteTaskCommentAsync(commentId);
                if (!result)
                {
                    return NotFound(new { message = "Comment not found" });
                }
                await _hubContext.Clients.Group($"task_{taskId}")
                    .SendAsync("TaskCommentDeleted", commentId);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                return StatusCode(500, new { message = "An error occurred while deleting comment" });
            }
        }
    }
}
