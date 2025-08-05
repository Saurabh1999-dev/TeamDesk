using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.Models.Entities
{
    public class User
    {
        // Primary key - unique identifier for each user
        public Guid Id { get; set; } = Guid.NewGuid();

        // Email must be unique and valid format
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // User's first name
        [Required]
        public string FirstName { get; set; } = string.Empty;

        // User's last name
        [Required]
        public string LastName { get; set; } = string.Empty;

        // Hashed password (never store plain text passwords!)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // User role: Staff, Manager, Admin
        [Required]
        public string Role { get; set; } = "Staff";

        // Whether user account is active
        public bool IsActive { get; set; } = true;

        // When user was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // When user was last updated
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
