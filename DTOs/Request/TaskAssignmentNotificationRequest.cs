using System.ComponentModel.DataAnnotations;

namespace TeamDesk.DTOs.Request
{
    public class TaskAssignmentNotificationRequest
    {
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public Guid AssignedToId { get; set; }

        [MaxLength(500)]
        public string? CustomMessage { get; set; }
    }
}
