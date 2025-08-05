using System.ComponentModel.DataAnnotations;

namespace TeamDesk.DTOs.Request
{
    public class UpdateStaffRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive")]
        public decimal Salary { get; set; }

        public List<string>? Skills { get; set; }
    }
}
