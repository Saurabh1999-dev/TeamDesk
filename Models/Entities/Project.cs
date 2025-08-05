using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;
using TeamDesk.Models.Entities;

namespace TeamDesk.DTOs
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public Guid ClientId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime Deadline { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

        [Range(0, 100)]
        public int Progress { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Client Client { get; set; } = null!;
        public List<ProjectStaffAssignment> StaffAssignments { get; set; } = new();
    }
}
