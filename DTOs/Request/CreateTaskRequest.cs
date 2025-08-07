using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.DTOs
{
    public class CreateTaskRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid ProjectId { get; set; }

        public Guid? AssignedToId { get; set; }

        public Enum.TaskStatus Status { get; set; } = Enum.TaskStatus.Todo;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? EstimatedHours { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}
