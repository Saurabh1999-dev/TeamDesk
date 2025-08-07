using System.ComponentModel.DataAnnotations;

namespace TeamDesk.Models.Entities
{
    public class TaskComment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TaskId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Task Task { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
