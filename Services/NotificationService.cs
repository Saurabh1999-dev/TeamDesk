// Services/NotificationService.cs - Enhanced with SignalR
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.Hubs;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            AppDbContext context,
            ILogger<NotificationService> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50) // Limit to recent 50 notifications
                    .ToListAsync();

                return notifications.Select(MapToNotificationResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    RelatedEntityId = request.RelatedEntityId,
                    RelatedEntityType = request.RelatedEntityType,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToNotificationResponse(notification);

                // ✅ Send real-time notification via SignalR
                await _hubContext.Clients.Group($"user_{request.UserId}")
                    .SendAsync("ReceiveNotification", response);

                _logger.LogInformation("Created and broadcasted notification for user {UserId}", request.UserId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task<bool> SendTaskAssignmentNotificationAsync(Guid taskId, Guid assignedToId, string? customMessage)
        {
            try
            {
                // Get task details
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null) return false;

                // Get assigned staff details
                var staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == assignedToId);

                if (staff == null) return false;

                // Create notification
                var notification = new Notification
                {
                    UserId = staff.User.Id,
                    Title = "New Task Assigned",
                    Message = customMessage ?? $"You have been assigned a new task: '{task.Title}' in project '{task.Project?.Name}'.",
                    Type = "task_assigned",
                    RelatedEntityId = taskId,
                    RelatedEntityType = "task",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToNotificationResponse(notification);

                // ✅ Send real-time notification with special sound indicator
                await _hubContext.Clients.Group($"user_{staff.User.Id}")
                    .SendAsync("ReceiveNotification", response, "task_assigned"); // ✅ Pass notification type for sound

    

                _logger.LogInformation("Sent task assignment notification for task {TaskId} to user {UserId}",
                    taskId, staff.User.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending task assignment notification");
                return false;
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null) return false;

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                // ✅ Broadcast notification read status to user
                await _hubContext.Clients.Group($"user_{notification.UserId}")
                    .SendAsync("NotificationMarkedAsRead", notificationId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(Guid userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // ✅ Broadcast all notifications marked as read
                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("AllNotificationsMarkedAsRead");

                return unreadNotifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null) return false;

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                // ✅ Broadcast notification deletion to user
                await _hubContext.Clients.Group($"user_{notification.UserId}")
                    .SendAsync("NotificationDeleted", notificationId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw;
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                throw;
            }
        }

        private static NotificationResponse MapToNotificationResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
