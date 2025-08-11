using TeamDesk.Enum;

namespace TeamDesk.DTOs.Response
{
    // DTOs/Response/LeaveResponses.cs
    public class LeaveResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }        // ✅ Changed from StaffId to UserId
        public string UserName { get; set; }    // ✅ Changed from StaffName to UserName
        public string UserEmail { get; set; }   // ✅ Changed from StaffEmail to UserEmail
        public int UserRole { get; set; }    // ✅ Added UserRole to show Admin/HR/Staff
        public LeaveType LeaveType { get; set; }
        public string LeaveTypeString { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; }
        public LeaveStatus Status { get; set; }
        public string StatusString { get; set; }
        public Guid? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalComments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<LeaveAttachmentResponse> Attachments { get; set; } = new();
    }


    public class LeaveAttachmentResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public string UploadedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LeaveStatsResponse
    {
        public int TotalLeaves { get; set; }
        public int PendingLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
        public int RejectedLeaves { get; set; }
        public Dictionary<string, int> ByType { get; set; } = new();
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public int RemainingAnnualLeaves { get; set; }
    }
}
