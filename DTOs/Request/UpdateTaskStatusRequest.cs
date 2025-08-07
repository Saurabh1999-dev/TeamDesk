using System.ComponentModel.DataAnnotations;

namespace TeamDesk.DTOs.Request
{
    public class UpdateTaskStatusRequest
    {
        [Required]
        public Enum.TaskStatus Status { get; set; }
    }
}
