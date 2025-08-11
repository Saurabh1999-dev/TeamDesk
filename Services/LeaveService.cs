// Services/LeaveService.cs - Complete Implementation
using Microsoft.EntityFrameworkCore;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.DTOs.Response;
using TeamDesk.Enum;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeaveService> _logger;
        private readonly IFileUploadService _fileUploadService;
        private readonly INotificationService _notificationService;

        public LeaveService(
            AppDbContext context,
            ILogger<LeaveService> logger,
            IFileUploadService fileUploadService,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
            _notificationService = notificationService;
        }

        // ✅ Your existing methods...
        public async Task<List<LeaveResponse>> GetLeavesByUserAsync(Guid userId) // ✅ Renamed method
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.ApprovedBy)
                    .Include(l => l.Attachments)
                        .ThenInclude(a => a.UploadedBy)
                    .Where(l => l.UserId == userId && l.IsActive)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                return leaves.Select(MapToLeaveResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaves for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<LeaveResponse>> GetAllLeavesAsync()
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.ApprovedBy)
                    .Include(l => l.Attachments)
                        .ThenInclude(a => a.UploadedBy)
                    .Where(l => l.IsActive)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                return leaves.Select(MapToLeaveResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all leaves");
                throw;
            }
        }

        // ✅ ADD MISSING GetLeaveByIdAsync method
        public async Task<LeaveResponse?> GetLeaveByIdAsync(Guid id)
        {
            try
            {
                var leave = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.ApprovedBy)
                    .Include(l => l.Attachments)
                        .ThenInclude(a => a.UploadedBy)
                    .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);

                return leave != null ? MapToLeaveResponse(leave) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave with ID {LeaveId}", id);
                throw;
            }
        }

        public async Task<List<LeaveResponse>> GetPendingLeavesAsync()
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.ApprovedBy)
                    .Include(l => l.Attachments)
                        .ThenInclude(a => a.UploadedBy)
                    .Where(l => l.Status == LeaveStatus.Pending && l.IsActive)
                    .OrderBy(l => l.CreatedAt)
                    .ToListAsync();

                return leaves.Select(MapToLeaveResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending leaves");
                throw;
            }
        }

        public async Task<LeaveResponse> CreateLeaveAsync(CreateLeaveRequest request, Guid userId)
        {
            try
            {
                var user = await _context.User.FindAsync(userId);
                if (user == null)
                    throw new InvalidOperationException($"User with ID {userId} not found");

                if (request.StartDate.Date < DateTime.UtcNow.Date)
                    throw new InvalidOperationException("Leave start date cannot be in the past");

                if (request.EndDate.Date < request.StartDate.Date)
                    throw new InvalidOperationException("Leave end date must be after start date");

                var totalDays = (int)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;

                var overlappingLeave = await _context.Leaves
                    .Where(l => l.UserId == userId &&
                                l.IsActive &&
                                l.Status != LeaveStatus.Rejected &&
                                l.Status != LeaveStatus.Cancelled &&
                                ((request.StartDate >= l.StartDate && request.StartDate <= l.EndDate) ||
                                 (request.EndDate >= l.StartDate && request.EndDate <= l.EndDate) ||
                                 (request.StartDate <= l.StartDate && request.EndDate >= l.EndDate)))
                    .FirstOrDefaultAsync();

                if (overlappingLeave != null)
                    throw new InvalidOperationException("Leave dates overlap with existing leave application");

                var remainingDays = await GetRemainingLeaveDaysAsync(userId, request.LeaveType);
                if (totalDays > remainingDays && request.LeaveType == LeaveType.Annual)
                    throw new InvalidOperationException($"Insufficient leave balance. Remaining: {remainingDays} days");

                var leave = new Leave
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LeaveType = request.LeaveType,
                    StartDate = request.StartDate.Date,
                    EndDate = request.EndDate.Date,
                    TotalDays = totalDays,
                    Reason = request.Reason,
                    Status = LeaveStatus.Pending,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();

                await _notificationService.SendLeaveApplicationNotificationAsync(leave.Id);

                var createdLeave = await GetLeaveByIdAsync(leave.Id);
                _logger.LogInformation("Created leave application for user {UserId} from {StartDate} to {EndDate}",
                    userId, request.StartDate, request.EndDate);

                return createdLeave!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave application for user {UserId}", userId);
                throw;
            }
        }

        // ✅ ADD MISSING UpdateLeaveAsync method
        public async Task<LeaveResponse> UpdateLeaveAsync(UpdateLeaveRequest request, Guid userId)
        {
            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (leave == null)
                throw new InvalidOperationException("Leave application not found or access denied.");

            // Optional: Add business logic validation (e.g., cannot update past leaves)
            var totalDays = (int)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;
            // Update fields
            leave.StartDate = request.StartDate;
            leave.EndDate = request.EndDate;
            leave.Reason = request.Reason;
            leave.LeaveType = request.LeaveType;
            leave.Status = LeaveStatus.Pending;
            leave.TotalDays = totalDays;
            leave.UpdatedAt = DateTime.UtcNow;

            _context.Leaves.Update(leave);
            await _context.SaveChangesAsync();

            return new LeaveResponse
            {
                Id = leave.UserId,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                LeaveType = leave.LeaveType,
                Status = leave.Status
            };
        }

        public async Task<bool> CancelLeaveAsync(Guid userId)
        {
            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l =>l.UserId == userId);

            if (leave == null)
                throw new InvalidOperationException("Leave application not found or access denied.");


            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LeaveResponse> ApproveLeaveAsync(ApproveLeaveRequest request, Guid adminId)
        {
            try
            {
                var leave = await _context.Leaves
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.Id == request.LeaveId && l.IsActive);

                if (leave == null)
                    throw new InvalidOperationException($"Leave with ID {request.LeaveId} not found");

                //if (leave.Status != LeaveStatus.Pending)
                //    throw new InvalidOperationException($"Leave is already {leave.Status}");

                var admin = await _context.User.FindAsync(adminId);
                if (admin == null)
                    throw new InvalidOperationException($"Admin user with ID {adminId} not found");

                if (request.Status == LeaveStatus.Rejected && string.IsNullOrWhiteSpace(request.Comments))
                    throw new InvalidOperationException("Comments are required when rejecting a leave application");

                leave.Status = request.Status;
                leave.ApprovedById = adminId;
                leave.ApprovedAt = DateTime.UtcNow;
                leave.ApprovalComments = request.Comments?.Trim();
                leave.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _notificationService.SendLeaveStatusUpdateNotificationAsync(
                    leave.Id,
                    leave.UserId,
                    request.Status,
                    request.Comments
                );

                var updatedLeave = await GetLeaveByIdAsync(leave.Id);
                _logger.LogInformation("Updated leave {LeaveId} status to {Status} by admin {AdminId}",
                    leave.Id, request.Status, adminId);

                return updatedLeave!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving/rejecting leave");
                throw;
            }
        }

        // ✅ ADD MISSING UploadLeaveAttachmentAsync method
        public async Task<LeaveAttachmentResponse> UploadLeaveAttachmentAsync(Guid leaveId, IFormFile file, Guid uploadedById)
        {
            try
            {
                var leave = await _context.Leaves
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.Id == leaveId && l.IsActive);

                if (leave == null)
                    throw new InvalidOperationException($"Leave with ID {leaveId} not found");

                // Only allow upload for pending leaves or by the owner
                var user = await _context.User.FindAsync(uploadedById);
                if (user == null)
                    throw new InvalidOperationException($"User with ID {uploadedById} not found");

                // Check if user can upload to this leave
                if (leave.UserId != uploadedById && user.Role != 2 && user.Role != 3)
                    throw new InvalidOperationException("You can only upload attachments to your own leave applications");

                // Validate file
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                    throw new InvalidOperationException("File size cannot exceed 5MB");

                // Upload file
                var filePath = await _fileUploadService.UploadFileAsync(file, "leaves");
                var fileUrl = _fileUploadService.GetFileUrl(filePath);

                var attachment = new LeaveAttachment
                {
                    LeaveId = leaveId,
                    FileName = Path.GetFileName(filePath),
                    OriginalFileName = file.FileName,
                    FileType = file.ContentType,
                    FilePath = filePath,
                    FileUrl = fileUrl,
                    FileSize = file.Length,
                    UploadedById = uploadedById,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeaveAttachments.Add(attachment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Uploaded attachment {FileName} for leave {LeaveId}", file.FileName, leaveId);

                return new LeaveAttachmentResponse
                {
                    Id = attachment.Id,
                    LeaveId = attachment.LeaveId,
                    FileName = attachment.FileName,
                    OriginalFileName = attachment.OriginalFileName,
                    FileType = attachment.FileType,
                    FileUrl = attachment.FileUrl,
                    FileSize = attachment.FileSize,
                    UploadedByName = $"{user.FirstName} {user.LastName}",
                    CreatedAt = attachment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for leave {LeaveId}", leaveId);
                throw;
            }
        }

        // ✅ ADD MISSING DeleteLeaveAttachmentAsync method
        public async Task<bool> DeleteLeaveAttachmentAsync(Guid leaveId, Guid attachmentId)
        {
            try
            {
                var attachment = await _context.LeaveAttachments
                    .Include(a => a.Leave)
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && a.LeaveId == leaveId && a.IsActive);

                if (attachment == null)
                {
                    _logger.LogWarning("Attachment {AttachmentId} not found for leave {LeaveId}", attachmentId, leaveId);
                    return false;
                }

                // Delete physical file
                try
                {
                    await _fileUploadService.DeleteFileAsync(attachment.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete physical file {FilePath}", attachment.FilePath);
                }

                // Soft delete from database
                attachment.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted attachment {AttachmentId} for leave {LeaveId}", attachmentId, leaveId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for leave {LeaveId}", attachmentId, leaveId);
                throw;
            }
        }

        // ✅ ADD MISSING GetLeaveAttachmentsAsync method
        public async Task<List<LeaveAttachmentResponse>> GetLeaveAttachmentsAsync(Guid leaveId)
        {
            try
            {
                var attachments = await _context.LeaveAttachments
                    .Include(a => a.UploadedBy)
                    .Where(a => a.LeaveId == leaveId && a.IsActive)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return attachments.Select(a => new LeaveAttachmentResponse
                {
                    Id = a.Id,
                    LeaveId = a.LeaveId,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FileType = a.FileType,
                    FileUrl = a.FileUrl,
                    FileSize = a.FileSize,
                    UploadedByName = a.UploadedBy != null
                        ? $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}"
                        : "Unknown",
                    CreatedAt = a.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for leave {LeaveId}", leaveId);
                throw;
            }
        }

        // ✅ ADD MISSING GetLeavesByStatusAsync method
        public async Task<List<LeaveResponse>> GetLeavesByStatusAsync(LeaveStatus status)
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.ApprovedBy)
                    .Include(l => l.Attachments)
                        .ThenInclude(a => a.UploadedBy)
                    .Where(l => l.Status == status && l.IsActive)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                return leaves.Select(MapToLeaveResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaves by status {Status}", status);
                throw;
            }
        }

        // ✅ ADD MISSING GetLeaveStatsAsync method
        public async Task<LeaveStatsResponse> GetLeaveStatsAsync(Guid? userId = null)
        {
            try
            {
                var query = _context.Leaves.Where(l => l.IsActive);

                // Filter by user if specified
                if (userId.HasValue)
                {
                    query = query.Where(l => l.UserId == userId.Value);
                }

                var totalLeaves = await query.CountAsync();
                var pendingLeaves = await query.CountAsync(l => l.Status == LeaveStatus.Pending);
                var approvedLeaves = await query.CountAsync(l => l.Status == LeaveStatus.Approved);
                var rejectedLeaves = await query.CountAsync(l => l.Status == LeaveStatus.Rejected);

                var byType = await query
                    .GroupBy(l => l.LeaveType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type.ToString(), x => x.Count);

                var byStatus = await query
                    .GroupBy(l => l.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count);

                // Calculate remaining annual leave days for specific user
                var remainingAnnualLeaves = 0;
                if (userId.HasValue)
                {
                    remainingAnnualLeaves = await GetRemainingLeaveDaysAsync(userId.Value, LeaveType.Annual);
                }

                return new LeaveStatsResponse
                {
                    TotalLeaves = totalLeaves,
                    PendingLeaves = pendingLeaves,
                    ApprovedLeaves = approvedLeaves,
                    RejectedLeaves = rejectedLeaves,
                    ByType = byType,
                    ByStatus = byStatus,
                    RemainingAnnualLeaves = remainingAnnualLeaves
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave statistics");
                throw;
            }
        }

        public async Task<int> GetRemainingLeaveDaysAsync(Guid userId, LeaveType leaveType)
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;

                var approvedLeaves = await _context.Leaves
                    .Where(l => l.UserId == userId &&
                               l.LeaveType == leaveType &&
                               l.Status == LeaveStatus.Approved &&
                               l.StartDate.Year == currentYear &&
                               l.IsActive)
                    .SumAsync(l => l.TotalDays);

                var user = await _context.User.FindAsync(userId);
                var totalEntitlement = GetLeaveEntitlementByRole(user?.Role, leaveType);

                return Math.Max(0, totalEntitlement - approvedLeaves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating remaining leave days for user {UserId}", userId);
                return 0;
            }
        }

        private static int GetLeaveEntitlementByRole(int? userRole, LeaveType leaveType)
        {
            return leaveType switch
            {
                LeaveType.Annual => userRole switch
                {
                    2 => 25,
                    3 => 23,
                    1 => 21,
                    _ => 21
                },
                LeaveType.Sick => 10,
                LeaveType.Personal => userRole switch
                {
                    2 or 3 => 7,
                    1 => 5,
                    _ => 5
                },
                _ => 0
            };
        }

        private static LeaveResponse MapToLeaveResponse(Leave leave)
        {
            return new LeaveResponse
            {
                Id = leave.Id,
                UserId = leave.UserId,
                UserName = leave.User != null
                    ? $"{leave.User.FirstName} {leave.User.LastName}"
                    : "Unknown",
                UserEmail = leave.User?.Email,
                UserRole = leave.User.Role,
                LeaveType = leave.LeaveType,
                LeaveTypeString = LeaveExtensions.GetLeaveTypeString(leave.LeaveType),
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                TotalDays = leave.TotalDays,
                Reason = leave.Reason,
                Status = leave.Status,
                StatusString = LeaveExtensions.GetLeaveStatusString(leave.Status),
                ApprovedById = leave.ApprovedById,
                ApprovedByName = leave.ApprovedBy != null
                    ? $"{leave.ApprovedBy.FirstName} {leave.ApprovedBy.LastName}"
                    : null,
                ApprovedAt = leave.ApprovedAt,
                ApprovalComments = leave.ApprovalComments,
                CreatedAt = leave.CreatedAt,
                UpdatedAt = leave.UpdatedAt,
                Attachments = leave.Attachments?.Where(a => a.IsActive).Select(a => new LeaveAttachmentResponse
                {
                    Id = a.Id,
                    LeaveId = a.LeaveId,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FileType = a.FileType,
                    FileUrl = a.FileUrl,
                    FileSize = a.FileSize,
                    UploadedByName = a.UploadedBy != null
                        ? $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}"
                        : "Unknown",
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<LeaveAttachmentResponse>()
            };
        }
    }
}
