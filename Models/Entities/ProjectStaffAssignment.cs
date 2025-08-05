using System.ComponentModel.DataAnnotations;
using TeamDesk.DTOs;
using TeamDesk.Enum;

namespace TeamDesk.Models.Entities
{
    public class ProjectStaffAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectId { get; set; }

        public Guid StaffId { get; set; }

        public ProjectRole Role { get; set; } = ProjectRole.TeamMember;

        [Range(0, 100)]
        public decimal AllocationPercentage { get; set; } = 100;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UnassignedDate { get; set; }

        public bool IsActive { get; set; } = true;

        public Project Project { get; set; } = null!;
        public Staff Staff { get; set; } = null!;
    }
}
