using System.ComponentModel.DataAnnotations;

namespace TeamDesk.DTOs.Request
{
    public class AddTaskCommentRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }
}
