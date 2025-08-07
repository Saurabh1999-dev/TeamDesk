using TeamDesk.DTOs;
using TeamDesk.DTOs.Response;

namespace TeamDesk.Services.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskResponse>> GetAllTasksAsync();
        Task<TaskResponse?> GetTaskByIdAsync(Guid id);
        Task<List<TaskResponse>> GetTasksByProjectAsync(Guid projectId);
        Task<List<TaskResponse>> GetTasksByAssigneeAsync(Guid staffId);
        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid createdById);
        Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<TaskResponse> AssignTaskAsync(Guid taskId, Guid assignedToId, bool sendNotification = true);
        Task<TaskResponse> UpdateTaskStatusAsync(Guid id, Enum.TaskStatus status);
        Task<TaskResponse> UpdateTaskProgressAsync(Guid id, decimal progress);
        Task<TaskCommentResponse> AddTaskCommentAsync(Guid taskId, string comment);
        Task<TaskStatsResponse> GetTaskStatsAsync();

        Task<TaskAttachmentResponse> UploadTaskAttachmentAsync(Guid taskId, IFormFile file, Guid uploadedById);
        Task<bool> DeleteTaskAttachmentAsync(Guid taskId, Guid attachmentId);
        Task<List<TaskAttachmentResponse>> GetTaskAttachmentsAsync(Guid taskId);

        Task<TaskCommentResponse> AddTaskCommentAsync(Guid taskId, string comment, Guid userId);
        Task<List<TaskCommentResponse>> GetTaskCommentsAsync(Guid taskId);
        Task<bool> DeleteTaskCommentAsync(Guid commentId);
    }
}
