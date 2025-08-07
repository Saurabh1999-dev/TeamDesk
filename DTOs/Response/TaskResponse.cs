using TeamDesk.DTOs.Response;
using TeamDesk.Enum;

namespace TeamDesk.DTOs
{
    public class TaskResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedToEmail { get; set; }
        public Guid? CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public Enum.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal Progress { get; set; }
        public bool IsOverdue { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<TaskAttachmentResponse> Attachments { get; set; } = new();
        public List<TaskCommentResponse> Comments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TaskCommentResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TaskStatsResponse
    {
        public int Total { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, int> ByPriority { get; set; } = new();
        public int Overdue { get; set; }
    }
}
