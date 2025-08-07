using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;

namespace TeamDesk.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId);
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task<bool> SendTaskAssignmentNotificationAsync(Guid taskId, Guid assignedToId, string? customMessage);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
        Task<int> MarkAllNotificationsAsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId);
        Task<int> GetUnreadNotificationCountAsync(Guid userId);
    }
}
