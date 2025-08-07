//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using TeamDesk.DTOs;
//using TeamDesk.DTOs.Request;
//using TeamDesk.Services.Interfaces;

//namespace TeamDesk.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class NotificationsController : ControllerBase
//    {
//        private readonly INotificationService _notificationService;
//        private readonly ILogger<NotificationsController> _logger;

//        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
//        {
//            _notificationService = notificationService;
//            _logger = logger;
//        }

//        /// <summary>
//        /// Get notifications for a specific user
//        /// </summary>
//        [HttpGet("user/{userId}")]
//        public async Task<ActionResult<List<NotificationResponse>>> GetUserNotifications(Guid userId)
//        {
//            try
//            {
//                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
//                return Ok(notifications);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
//                return StatusCode(500, new { message = "An error occurred while retrieving notifications" });
//            }
//        }

//        /// <summary>
//        /// Create a new notification
//        /// </summary>
//        [HttpPost]
//        [Authorize(Roles = "Admin,HR")]
//        public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] CreateNotificationRequest request)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var notification = await _notificationService.CreateNotificationAsync(request);
//                return CreatedAtAction(nameof(GetUserNotifications), new { userId = notification.UserId }, notification);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating notification");
//                return StatusCode(500, new { message = "An error occurred while creating notification" });
//            }
//        }

//        /// <summary>
//        /// Send task assignment notification
//        /// </summary>
//        [HttpPost("task-assignment")]
//        [Authorize]
//        public async Task<ActionResult<NotificationResponse>> SendTaskAssignmentNotification([FromBody] TaskAssignmentNotificationRequest request)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var result = await _notificationService.SendTaskAssignmentNotificationAsync(
//                    request.TaskId,
//                    request.AssignedToId,
//                    request.CustomMessage);

//                return Ok(new { success = true, message = "Task assignment notification sent successfully" });
//            }
//            catch (InvalidOperationException ex)
//            {
//                return BadRequest(new { message = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error sending task assignment notification");
//                return StatusCode(500, new { message = "An error occurred while sending notification" });
//            }
//        }

//        /// <summary>
//        /// Mark notification as read
//        /// </summary>
//        [HttpPut("{notificationId}/read")]
//        [Authorize]
//        public async Task<ActionResult> MarkNotificationAsRead(Guid notificationId)
//        {
//            try
//            {
//                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);
//                if (!result)
//                {
//                    return NotFound(new { message = $"Notification with ID {notificationId} not found" });
//                }

//                return Ok(new { success = true });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking notification as read with ID {NotificationId}", notificationId);
//                return StatusCode(500, new { message = "An error occurred while marking notification as read" });
//            }
//        }

//        /// <summary>
//        /// Mark all notifications as read for a user
//        /// </summary>
//        [HttpPut("user/{userId}/read-all")]
//        [Authorize]
//        public async Task<ActionResult> MarkAllNotificationsAsRead(Guid userId)
//        {
//            try
//            {
//                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
//                return Ok(new { success = true, markedCount = result });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
//                return StatusCode(500, new { message = "An error occurred while marking notifications as read" });
//            }
//        }

//        /// <summary>
//        /// Delete notification
//        /// </summary>
//        [HttpDelete("{notificationId}")]
//        [Authorize]
//        public async Task<ActionResult> DeleteNotification(Guid notificationId)
//        {
//            try
//            {
//                var result = await _notificationService.DeleteNotificationAsync(notificationId);
//                if (!result)
//                {
//                    return NotFound(new { message = $"Notification with ID {notificationId} not found" });
//                }

//                return Ok(new { success = true });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting notification with ID {NotificationId}", notificationId);
//                return StatusCode(500, new { message = "An error occurred while deleting notification" });
//            }
//        }

//        /// <summary>
//        /// Get unread notification count for a user
//        /// </summary>
//        [HttpGet("user/{userId}/unread-count")]
//        public async Task<ActionResult<int>> GetUnreadNotificationCount(Guid userId)
//        {
//            try
//            {
//                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
//                return Ok(count);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving unread notification count for user {UserId}", userId);
//                return StatusCode(500, new { message = "An error occurred while retrieving notification count" });
//            }
//        }
//    }
//}
