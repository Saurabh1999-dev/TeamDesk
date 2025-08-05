using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.DTOs
{
    public class CreateProjectRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

        public List<ProjectStaffAssignmentRequest> StaffAssignments { get; set; } = new();
    }

    public class UpdateProjectRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        public ProjectStatus Status { get; set; }

        public ProjectPriority Priority { get; set; }

        [Range(0, 100)]
        public int Progress { get; set; }
    }

    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ClientResponse Client { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int Progress { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
        public List<ProjectStaffAssignmentResponse> StaffAssignments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class ProjectStaffAssignmentRequest
    {
        [Required]
        public Guid StaffId { get; set; }

        public ProjectRole Role { get; set; } = ProjectRole.TeamMember;

        [Range(0, 100)]
        public decimal AllocationPercentage { get; set; } = 100;
    }

    public class ProjectStaffAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffEmail { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal AllocationPercentage { get; set; }
        public DateTime AssignedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClientResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }

    public class CreateClientRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
    }
}
