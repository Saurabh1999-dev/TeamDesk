using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.DTOs.Request
{
    public class CreateLeaveRequest
    {
        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; }
    }

    public class UpdateLeaveRequest
    {
        public LeaveType? LeaveType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; }
    }

    public class ApproveLeaveRequest
    {
        [Required]
        public Guid LeaveId { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } // Approved or Rejected

        [StringLength(1000)]
        public string? Comments { get; set; } // Required for rejection
    }
}
