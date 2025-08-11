using TeamDesk.DTOs.Request;
using TeamDesk.DTOs.Response;
using TeamDesk.Enum;

namespace TeamDesk.Services.Interfaces
{
    // Services/Interfaces/ILeaveService.cs - Update method signatures
    public interface ILeaveService
    {
        Task<List<LeaveResponse>> GetAllLeavesAsync();
        Task<LeaveResponse?> GetLeaveByIdAsync(Guid id);
        Task<List<LeaveResponse>> GetLeavesByUserAsync(Guid userId); // ✅ Renamed from GetLeavesByStaffAsync
        Task<List<LeaveResponse>> GetPendingLeavesAsync();
        Task<LeaveResponse> CreateLeaveAsync(CreateLeaveRequest request, Guid userId);
        Task<LeaveResponse> UpdateLeaveAsync(Guid id, UpdateLeaveRequest request, Guid userId);
        Task<bool> DeleteLeaveAsync(Guid id, Guid userId);
        Task<bool> CancelLeaveAsync(Guid id, Guid userId);

        // Admin Operations
        Task<LeaveResponse> ApproveLeaveAsync(ApproveLeaveRequest request, Guid adminId);
        Task<List<LeaveResponse>> GetLeavesByStatusAsync(LeaveStatus status);

        // Attachment Operations
        Task<LeaveAttachmentResponse> UploadLeaveAttachmentAsync(Guid leaveId, IFormFile file, Guid uploadedById);
        Task<bool> DeleteLeaveAttachmentAsync(Guid leaveId, Guid attachmentId);
        Task<List<LeaveAttachmentResponse>> GetLeaveAttachmentsAsync(Guid leaveId);

        // Statistics
        Task<LeaveStatsResponse> GetLeaveStatsAsync(Guid? userId = null);
        Task<int> GetRemainingLeaveDaysAsync(Guid userId, LeaveType leaveType); // ✅ Updated parameter
    }

}
