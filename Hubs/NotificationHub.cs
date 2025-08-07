// Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TeamDesk.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected to SignalR", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} disconnected from SignalR", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinNotificationGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
        }

        public async Task LeaveNotificationGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
        }

        public async Task MarkNotificationAsRead(string notificationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
            }
        }

        public async Task JoinTaskChat(string taskId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var groupName = $"task_{taskId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                Context.Items[$"task_{taskId}"] = true;

                _logger.LogInformation("User {UserId} joined task chat for task {TaskId}", userId, taskId);
                await Clients.OthersInGroup(groupName).SendAsync("UserJoinedTaskChat", new
                {
                    UserId = userId,
                    TaskId = taskId,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        public async Task LeaveTaskChat(string taskId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var groupName = $"task_{taskId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                Context.Items.Remove($"task_{taskId}");

                _logger.LogInformation("User {UserId} left task chat for task {TaskId}", userId, taskId);
                await Clients.OthersInGroup(groupName).SendAsync("UserLeftTaskChat", new
                {
                    UserId = userId,
                    TaskId = taskId,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        public async Task UserTyping(string taskId, string userName)
        {
            var groupName = $"task_{taskId}";
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new
            {
                TaskId = taskId,
                UserName = userName,
                Timestamp = DateTime.UtcNow
            });
        }
        public async Task UserStoppedTyping(string taskId, string userName)
        {
            var groupName = $"task_{taskId}";
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", new
            {
                TaskId = taskId,
                UserName = userName,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
