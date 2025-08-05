using System.ComponentModel.DataAnnotations;
using TeamDesk.DTOs;

namespace TeamDesk.Models.Entities
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public List<Project> Projects { get; set; } = new();
    }
}
