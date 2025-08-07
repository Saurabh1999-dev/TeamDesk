using Microsoft.EntityFrameworkCore;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Response;
using TeamDesk.Enum;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskService> _logger;
        private readonly IFileUploadService _fileUploadService;
        private readonly INotificationService _notificationService;

        public TaskService(AppDbContext context, ILogger<TaskService> logger, IFileUploadService fileUploadService, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
            _notificationService = notificationService;
        }

        public async Task<List<TaskResponse>> GetAllTasksAsync()
        {
            try
            {
                var tasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedTo)
                        .ThenInclude(s => s.User)
                    .Include(t => t.CreatedBy)
                        .ThenInclude(s => s.User)
                    .Where(t => t.IsActive)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return tasks.Select(MapToTaskResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                throw;
            }
        }

        public async Task<TaskAttachmentResponse> UploadTaskAttachmentAsync(Guid taskId, IFormFile file, Guid uploadedById)
        {
            try
            {
                // Validate task exists
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null || !task.IsActive)
                {
                    throw new InvalidOperationException($"Task with ID {taskId} not found or inactive");
                }

                // Validate user exists
                var user = await _context.User.FindAsync(uploadedById);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {uploadedById} not found");
                }

                // Upload file
                var filePath = await _fileUploadService.UploadFileAsync(file, "tasks");
                var fileUrl = _fileUploadService.GetFileUrl(filePath);

                // Create attachment record
                var attachment = new TaskAttachment
                {
                    TaskId = taskId,
                    FileName = Path.GetFileName(filePath),
                    OriginalFileName = file.FileName,
                    FileType = file.ContentType,
                    FilePath = filePath,
                    FileUrl = fileUrl,
                    FileSize = file.Length,
                    UploadedById = uploadedById,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TaskAttachments.Add(attachment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Uploaded attachment {FileName} for task {TaskId}", file.FileName, taskId);

                return new TaskAttachmentResponse
                {
                    Id = attachment.Id,
                    TaskId = attachment.TaskId,
                    FileName = attachment.FileName,
                    OriginalFileName = attachment.OriginalFileName,
                    FileType = attachment.FileType,
                    FileUrl = attachment.FileUrl,
                    FileSize = attachment.FileSize,
                    UploadedByName = $"{user.FirstName} {user.LastName}",
                    CreatedAt = attachment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for task {TaskId}", taskId);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAttachmentAsync(Guid taskId, Guid attachmentId)
        {
            try
            {
                var attachment = await _context.TaskAttachments
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == taskId && a.IsActive);

                if (attachment == null) return false;

                // Delete physical file
                await _fileUploadService.DeleteFileAsync(attachment.FilePath);

                // Soft delete from database
                attachment.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted attachment {AttachmentId} for task {TaskId}", attachmentId, taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for task {TaskId}", attachmentId, taskId);
                throw;
            }
        }

        public async Task<List<TaskAttachmentResponse>> GetTaskAttachmentsAsync(Guid taskId)
        {
            try
            {
                var attachments = await _context.TaskAttachments
                    .Include(a => a.UploadedBy)
                    .Where(a => a.TaskId == taskId && a.IsActive)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return attachments.Select(a => new TaskAttachmentResponse
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FileType = a.FileType,
                    FileUrl = a.FileUrl,
                    FileSize = a.FileSize,
                    UploadedByName = $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}",
                    CreatedAt = a.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for task {TaskId}", taskId);
                throw;
            }
        }

        public async Task<TaskResponse?> GetTaskByIdAsync(Guid id)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedTo)
                        .ThenInclude(s => s.User)
                    .Include(t => t.CreatedBy)
                        .ThenInclude(s => s.User)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

                return task != null ? MapToTaskResponse(task) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
                throw;
            }
        }

        public async Task<List<TaskResponse>> GetTasksByProjectAsync(Guid projectId)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedTo)
                        .ThenInclude(s => s.User)
                    .Where(t => t.ProjectId == projectId && t.IsActive)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return tasks.Select(MapToTaskResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<List<TaskResponse>> GetTasksByAssigneeAsync(Guid staffId)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedTo)
                        .ThenInclude(s => s.User)
                    .Where(t => t.AssignedToId == staffId && t.IsActive)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return tasks.Select(MapToTaskResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for assignee {AssigneeId}", staffId);
                throw;
            }
        }

        public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid createdById)
        {
            try
            {
                // Validate project exists
                var project = await _context.Projects.FindAsync(request.ProjectId);
                if (project == null || !project.IsActive)
                {
                    throw new InvalidOperationException($"Project with ID {request.ProjectId} not found or inactive");
                }

                // Validate assigned staff if provided
                if (request.AssignedToId.HasValue)
                {
                    var staff = await _context.Staff.FindAsync(request.AssignedToId.Value);
                    if (staff == null)
                    {
                        throw new InvalidOperationException($"Staff with ID {request.AssignedToId.Value} not found");
                    }
                }

                var task = new Models.Entities.Task
                {
                    Title = request.Title,
                    Description = request.Description,
                    ProjectId = request.ProjectId,
                    AssignedToId = request.AssignedToId,
                    Status = (int)request.Status,
                    Priority = (int)request.Priority,
                    StartDate = request.StartDate,
                    DueDate = request.DueDate,
                    EstimatedHours = request.EstimatedHours ?? 0,
                    Progress = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Tags = request.Tags ?? new List<string>(),
                    //CreatedById = createdById
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // Send notification if task is assigned
                if (request.AssignedToId.HasValue)
                {
                    await _notificationService.SendTaskAssignmentNotificationAsync(
                        task.Id,
                        request.AssignedToId.Value,
                        null);
                }

                // Reload task with related data
                var createdTask = await GetTaskByIdAsync(task.Id);
                _logger.LogInformation("Created new task: {TaskTitle} with ID {TaskId}",
                    task.Title, task.Id);

                return createdTask!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                throw;
            }
        }

        public async Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null || !task.IsActive)
                {
                    throw new InvalidOperationException($"Task with ID {id} not found or inactive");
                }

                // Update task properties
                if (!string.IsNullOrEmpty(request.Title))
                    task.Title = request.Title;

                if (!string.IsNullOrEmpty(request.Description))
                    task.Description = request.Description;

                // Handle assignment change
                if (request.AssignedToId.HasValue && task.AssignedToId != request.AssignedToId)
                {
                    // Validate new assignee
                    var staff = await _context.Staff.FindAsync(request.AssignedToId.Value);
                    if (staff == null)
                    {
                        throw new InvalidOperationException($"Staff with ID {request.AssignedToId.Value} not found");
                    }

                    task.AssignedToId = request.AssignedToId;

                    // Send notification to new assignee
                    await _notificationService.SendTaskAssignmentNotificationAsync(
                        task.Id,
                        request.AssignedToId.Value,
                        null);
                }

                if (request.Status.HasValue)
                {
                    task.Status = (int)request.Status.Value;

                    // Set completion date if status is completed
                    if (request.Status.Value == Enum.TaskStatus.Completed && !task.CompletedDate.HasValue)
                    {
                        task.CompletedDate = DateTime.UtcNow;
                    }
                }

                if (request.Priority.HasValue)
                    task.Priority = (int)request.Priority.Value;

                if (request.StartDate.HasValue)
                    task.StartDate = request.StartDate;

                if (request.DueDate.HasValue)
                    task.DueDate = request.DueDate;

                if (request.EstimatedHours.HasValue)
                    task.EstimatedHours = request.EstimatedHours.Value;

                if (request.ActualHours.HasValue)
                    task.ActualHours = request.ActualHours.Value;

                if (request.Tags != null)
                    task.Tags = request.Tags;

                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedTask = await GetTaskByIdAsync(id);
                _logger.LogInformation("Updated task: {TaskTitle} with ID {TaskId}",
                    task.Title, task.Id);

                return updatedTask!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task with ID {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null) return false;

                // Soft delete
                task.IsActive = false;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Soft deleted task with ID {TaskId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                throw;
            }
        }

        public async Task<TaskResponse> AssignTaskAsync(Guid taskId, Guid assignedToId, bool sendNotification = true)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null || !task.IsActive)
                {
                    throw new InvalidOperationException($"Task with ID {taskId} not found or inactive");
                }

                var staff = await _context.Staff.FindAsync(assignedToId);
                if (staff == null)
                {
                    throw new InvalidOperationException($"Staff with ID {assignedToId} not found");
                }

                task.AssignedToId = assignedToId;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                if (sendNotification)
                {
                    await _notificationService.SendTaskAssignmentNotificationAsync(
                        taskId,
                        assignedToId,
                        null);
                }

                var updatedTask = await GetTaskByIdAsync(taskId);
                _logger.LogInformation("Assigned task {TaskId} to staff {StaffId}",
                    taskId, assignedToId);

                return updatedTask!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task");
                throw;
            }
        }

        public async Task<TaskResponse> UpdateTaskStatusAsync(Guid id, Enum.TaskStatus status)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null || !task.IsActive)
                {
                    throw new InvalidOperationException($"Task with ID {id} not found or inactive");
                }

                task.Status = (int)status;

                // Set completion date if status is completed
                if (status == Enum.TaskStatus.Completed && !task.CompletedDate.HasValue)
                {
                    task.CompletedDate = DateTime.UtcNow;
                }

                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedTask = await GetTaskByIdAsync(id);
                _logger.LogInformation("Updated task status {TaskId} to {Status}",
                    id, status);

                return updatedTask!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status");
                throw;
            }
        }

        public async Task<TaskResponse> UpdateTaskProgressAsync(Guid id, decimal progress)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null || !task.IsActive)
                {
                    throw new InvalidOperationException($"Task with ID {id} not found or inactive");
                }

                task.Progress = progress;
                task.UpdatedAt = DateTime.UtcNow;

                // Auto-update status based on progress
                if (progress >= 100 && task.Status != (int)Enum.TaskStatus.Completed)
                {
                    task.Status = (int)Enum.TaskStatus.Completed;
                    task.CompletedDate = DateTime.UtcNow;
                }
                else if (progress > 0 && task.Status == (int)Enum.TaskStatus.Todo)
                {
                    task.Status = (int)Enum.TaskStatus.InProgress;
                }

                await _context.SaveChangesAsync();

                var updatedTask = await GetTaskByIdAsync(id);
                _logger.LogInformation("Updated task progress {TaskId} to {Progress}%",
                    id, progress);

                return updatedTask!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task progress");
                throw;
            }
        }

        public async Task<TaskCommentResponse> AddTaskCommentAsync(Guid taskId, string comment)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null || !task.IsActive)
                {
                    throw new InvalidOperationException($"Task with ID {taskId} not found or inactive");
                }

                // TODO: Get current user ID from HTTP context
                var currentUserId = Guid.NewGuid(); // This should be replaced with actual user ID

                var taskComment = new TaskComment
                {
                    TaskId = taskId,
                    UserId = currentUserId,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TaskComments.Add(taskComment);
                await _context.SaveChangesAsync();

                // Get user details for response
                var user = await _context.User.FindAsync(currentUserId);

                _logger.LogInformation("Added comment to task {TaskId}", taskId);

                return new TaskCommentResponse
                {
                    Id = taskComment.Id,
                    TaskId = taskComment.TaskId,
                    UserId = taskComment.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                    Comment = taskComment.Comment,
                    CreatedAt = taskComment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding task comment");
                throw;
            }
        }

        public async Task<TaskStatsResponse> GetTaskStatsAsync()
        {
            try
            {
                var total = await _context.Tasks.CountAsync(t => t.IsActive);

                var byStatus = await _context.Tasks
                    .Where(t => t.IsActive)
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var byPriority = await _context.Tasks
                    .Where(t => t.IsActive)
                    .GroupBy(t => t.Priority)
                    .Select(g => new { Priority = g.Key, Count = g.Count() })
                    .ToListAsync();

                var overdue = await _context.Tasks
                    .Where(t => t.IsActive &&
                               t.DueDate.HasValue &&
                               t.DueDate.Value < DateTime.UtcNow &&
                               t.Status != (int)Enum.TaskStatus.Completed)
                    .CountAsync();

                return new TaskStatsResponse
                {
                    Total = total,
                    ByStatus = byStatus.ToDictionary(
                        x => ((Enum.TaskStatus)x.Status).ToString(),
                        x => x.Count),
                    ByPriority = byPriority.ToDictionary(
                        x => ((TaskPriority)x.Priority).ToString(),
                        x => x.Count),
                    Overdue = overdue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task statistics");
                throw;
            }
        }

        // Private helper method to map Task entity to TaskResponse DTO
        private static TaskResponse MapToTaskResponse(Models.Entities.Task task)
        {
            var today = DateTime.UtcNow.Date;
            var isOverdue = task.DueDate.HasValue &&
                           task.DueDate.Value.Date < today &&
                           task.Status != (int)Enum.TaskStatus.Completed;

            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? string.Empty,
                AssignedToId = task.AssignedToId,
                AssignedToName = task.AssignedTo != null ?
                    $"{task.AssignedTo.User.FirstName} {task.AssignedTo.User.LastName}" : null,
                AssignedToEmail = task.AssignedTo?.User?.Email,
                //CreatedById = task.CreatedById,
                CreatedByName = task.CreatedBy != null ?
                    $"{task.CreatedBy.User.FirstName} {task.CreatedBy.User.LastName}" : null,
                Status = (Enum.TaskStatus)task.Status,
                Priority = (TaskPriority)task.Priority,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                CompletedDate = task.CompletedDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                Progress = task.Progress,
                IsOverdue = isOverdue,
                Tags = task.Tags ?? new List<string>(),
                Attachments = task.Attachments?.Where(a => a.IsActive).Select(a => new TaskAttachmentResponse
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FileType = a.FileType,
                    FileUrl = a.FileUrl,
                    FileSize = a.FileSize,
                    UploadedByName = a.UploadedBy != null ? $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}" : "Unknown",
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<TaskAttachmentResponse>(),
                Comments = task.Comments?.Select(c => new TaskCommentResponse
                {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown User",
                    Comment = c.Comment,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new List<TaskCommentResponse>(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
