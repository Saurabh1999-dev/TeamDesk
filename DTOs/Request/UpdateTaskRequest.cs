using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

public class UpdateTaskRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public Guid? AssignedToId { get; set; }

    public TeamDesk.Enum.TaskStatus? Status { get; set; }

    public TaskPriority? Priority { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedHours { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ActualHours { get; set; }

    public List<string>? Tags { get; set; }
}