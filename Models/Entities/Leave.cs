// Models/Entities/Leave.cs
using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.Models.Entities
{
    public class Leave
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // ✅ Changed from StaffId to UserId
        public User User { get; set; }   // ✅ Changed from Staff to User

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int TotalDays { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public Guid? ApprovedById { get; set; }
        public User ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(1000)]
        public string? ApprovalComments { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LeaveAttachment> Attachments { get; set; } = new List<LeaveAttachment>();
    }
}
