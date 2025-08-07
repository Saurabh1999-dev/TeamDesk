using System.ComponentModel.DataAnnotations;
using TeamDesk.DTOs;
using TeamDesk.Enum;

namespace TeamDesk.Models.Entities
{
    public class Task
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public Guid ProjectId { get; set; }

        public Guid? AssignedToId { get; set; }

        public Guid? CreatedById { get; set; }

        public int Status { get; set; } = (int)Enum.TaskStatus.Todo;

        public int Priority { get; set; } = (int)TaskPriority.Medium;

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal EstimatedHours { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal ActualHours { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;

        public Staff? AssignedTo { get; set; }

        public Staff CreatedBy { get; set; }

        public List<TaskComment> Comments { get; set; } = new();
        public List<TaskAttachment> Attachments { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }
}
