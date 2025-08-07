using System.ComponentModel.DataAnnotations;

namespace TeamDesk.DTOs.Request
{
    public class AssignTaskRequest
    {
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public Guid AssignedToId { get; set; }

        public bool SendNotification { get; set; } = true;
    }
}
