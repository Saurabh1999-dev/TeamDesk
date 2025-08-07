using System.ComponentModel.DataAnnotations;

namespace TeamDesk.DTOs.Request
{
    public class UpdateTaskProgressRequest
    {
        [Required]
        [Range(0, 100)]
        public decimal Progress { get; set; }
    }

}
