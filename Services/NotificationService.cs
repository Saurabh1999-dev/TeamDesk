// Services/NotificationService.cs - Enhanced with SignalR
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.Enum;
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
                    .Take(50)
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
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null) return false;

                var staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == assignedToId);

                if (staff == null) return false;

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
                await _hubContext.Clients.Group($"user_{staff.User.Id}")
                    .SendAsync("ReceiveNotification", response, "task_assigned");

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

        public async Task<bool> SendLeaveApplicationNotificationAsync(Guid leaveId)
        {
            try
            {
                // Get leave details with related data
                var leave = await _context.Leaves
                    .Include(l => l.User)
                        .ThenInclude(s => s.Id)
                    .FirstOrDefaultAsync(l => l.Id == leaveId);

                if (leave == null)
                {
                    _logger.LogWarning("Leave with ID {LeaveId} not found for notification", leaveId);
                    return false;
                }
                var adminUsers = await _context.User
                    .Where(u => u.Role == (int)UserRole.Admin || u.Role == (int)UserRole.HR)
                    .ToListAsync();

                if (!adminUsers.Any())
                {
                    _logger.LogWarning("No admin/HR users found to notify about leave application");
                    return false;
                }

                var staffName = $"{leave.User.FirstName} {leave.User.LastName}";
                var leaveTypeString = LeaveExtensions.GetLeaveTypeString(leave.LeaveType);

                // Format date range
                var dateRange = leave.StartDate.Date == leave.EndDate.Date
                    ? leave.StartDate.ToString("MMM dd, yyyy")
                    : $"{leave.StartDate:MMM dd} - {leave.EndDate:MMM dd, yyyy}";

                // Create notifications for each admin/HR user
                var notifications = new List<Notification>();

                foreach (var admin in adminUsers)
                {
                    var notification = new Notification
                    {
                        UserId = admin.Id,
                        Title = "New Leave Application",
                        Message = $"{staffName} has applied for {leaveTypeString} from {dateRange} ({leave.TotalDays} days). Reason: {leave.Reason}",
                        Type = "leave_application",
                        RelatedEntityId = leaveId,
                        RelatedEntityType = "leave",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    notifications.Add(notification);
                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                // Send real-time notifications to all admin/HR users
                foreach (var notification in notifications)
                {
                    var response = MapToNotificationResponse(notification);

                    // Send to specific user group
                    await _hubContext.Clients.Group($"user_{notification.UserId}")
                        .SendAsync("ReceiveNotification", response, "leave_application");
                }

                // Also send to admin/HR role groups if you have them
                await _hubContext.Clients.Groups("role_Admin", "role_HR")
                    .SendAsync("ReceiveLeaveApplication", new
                    {
                        LeaveId = leaveId,
                        StaffName = staffName,
                        LeaveType = leaveTypeString,
                        DateRange = dateRange,
                        TotalDays = leave.TotalDays,
                        Reason = leave.Reason,
                        CreatedAt = leave.CreatedAt
                    });

                _logger.LogInformation("Sent leave application notifications for leave {LeaveId} to {AdminCount} admin/HR users",
                    leaveId, adminUsers.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending leave application notification for leave {LeaveId}", leaveId);
                return false;
            }
        }

        public async Task<bool> SendLeaveStatusUpdateNotificationAsync(Guid leaveId, Guid staffId, LeaveStatus status, string? comments)
        {
            try
            {
                // Get leave details
                var leave = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.ApprovedBy)
                    .FirstOrDefaultAsync(l => l.Id == leaveId);

                if (leave == null)
                {
                    _logger.LogWarning("Leave with ID {LeaveId} not found for status update notification", leaveId);
                    return false;
                }

                // Get staff user details
                var staffUser = await _context.Staff
                    .Include(s => s.User)
                    .Where(s => s.Id == staffId)
                    .Select(s => s.User)
                    .FirstOrDefaultAsync();

                if (staffUser == null)
                {
                    _logger.LogWarning("Staff user with ID {StaffId} not found for notification", staffId);
                    return false;
                }

                var leaveTypeString = LeaveExtensions.GetLeaveTypeString(leave.LeaveType);
                var statusString = LeaveExtensions.GetLeaveStatusString(status);
                var dateRange = leave.StartDate.Date == leave.EndDate.Date
                    ? leave.StartDate.ToString("MMM dd, yyyy")
                    : $"{leave.StartDate:MMM dd} - {leave.EndDate:MMM dd, yyyy}";

                string title;
                string message;
                string notificationType;

                switch (status)
                {
                    case LeaveStatus.Approved:
                        title = "Leave Application Approved";
                        message = $"Your {leaveTypeString} application for {dateRange} has been approved.";
                        notificationType = "leave_approved";

                        if (!string.IsNullOrEmpty(comments))
                        {
                            message += $" Comments: {comments}";
                        }
                        break;

                    case LeaveStatus.Rejected:
                        title = "Leave Application Rejected";
                        message = $"Your {leaveTypeString} application for {dateRange} has been rejected.";
                        notificationType = "leave_rejected";

                        if (!string.IsNullOrEmpty(comments))
                        {
                            message += $" Reason: {comments}";
                        }
                        break;

                    default:
                        title = "Leave Status Updated";
                        message = $"Your {leaveTypeString} application for {dateRange} status has been updated to {statusString}.";
                        notificationType = "leave_status_updated";
                        break;
                }

                var notification = new Notification
                {
                    UserId = staffUser.Id,
                    Title = title,
                    Message = message,
                    Type = notificationType,
                    RelatedEntityId = leaveId,
                    RelatedEntityType = "leave",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var response = MapToNotificationResponse(notification);
                await _hubContext.Clients.Group($"user_{staffUser.Id}")
                    .SendAsync("ReceiveNotification", response, notificationType);

                await _hubContext.Clients.Group($"user_{staffUser.Id}")
                    .SendAsync("ReceiveLeaveStatusUpdate", new
                    {
                        LeaveId = leaveId,
                        Status = status.ToString(),
                        StatusString = statusString,
                        Comments = comments,
                        ApprovedBy = leave.ApprovedBy != null ? $"{leave.ApprovedBy.FirstName} {leave.ApprovedBy.LastName}" : null,
                        ApprovedAt = leave.ApprovedAt
                    });

                _logger.LogInformation("Sent leave status update notification for leave {LeaveId} to staff {StaffId} with status {Status}",
                    leaveId, staffId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending leave status update notification for leave {LeaveId}", leaveId);
                return false;
            }
        }
     
    }
}
