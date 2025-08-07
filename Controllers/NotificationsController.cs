using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUserNotifications(Guid userId)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPut("{notificationId}/mark-read")]
    public async Task<ActionResult> MarkNotificationAsRead(Guid notificationId)
    {
        var success = await _notificationService.MarkNotificationAsReadAsync(notificationId);
        return Ok(new { success });
    }

    [HttpPut("user/{userId}/mark-all-read")]
    public async Task<ActionResult> MarkAllNotificationsAsRead(Guid userId)
    {
        var count = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
        return Ok(new { success = true, count });
    }

    [HttpDelete("{notificationId}")]
    public async Task<ActionResult> DeleteNotification(Guid notificationId)
    {
        var success = await _notificationService.DeleteNotificationAsync(notificationId);
        return Ok(new { success });
    }

    [HttpGet("user/{userId}/unread-count")]
    public async Task<ActionResult> GetUnreadNotificationCount(Guid userId)
    {
        var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
        return Ok(new { count });
    }
}
