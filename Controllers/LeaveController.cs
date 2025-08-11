// Controllers/LeaveController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.DTOs.Response;
using TeamDesk.Enum;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(ILeaveService leaveService, ILogger<LeaveController> logger)
        {
            _leaveService = leaveService;
            _logger = logger;
        }

        // Get all leaves (Admin only)
        [HttpGet]
        public async Task<ActionResult<List<LeaveResponse>>> GetAllLeaves()
        {
            try
            {
                var leaves = await _leaveService.GetAllLeavesAsync();
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all leaves");
                return StatusCode(500, "An error occurred while retrieving leaves");
            }
        }

        // Get leave by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveResponse>> GetLeave(Guid id)
        {
            try
            {
                var leave = await _leaveService.GetLeaveByIdAsync(id);
                if (leave == null)
                {
                    return NotFound($"Leave with ID {id} not found");
                }

                // Check if user can access this leave
                var currentUserId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // ✅ Updated to check UserId instead of StaffId
                if (userRole != "Admin" && userRole != "HR" && leave.UserId.ToString() != currentUserId)
                {
                    return Forbid("You can only view your own leave applications");
                }

                return Ok(leave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave {LeaveId}", id);
                return StatusCode(500, "An error occurred while retrieving the leave");
            }
        }

        // Get my leaves (Staff)
        [HttpGet("my-leaves")]
        public async Task<ActionResult<List<LeaveResponse>>> GetMyLeaves()
        {
            try
            {
                var userId = Guid.Parse(GetCurrentUserId());
                var leaves = await _leaveService.GetLeavesByUserAsync(userId);
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's leaves");
                return StatusCode(500, "An error occurred while retrieving your leaves");
            }
        }

        // Get pending leaves (Admin/HR only)
        [HttpGet("pending")]
        public async Task<ActionResult<List<LeaveResponse>>> GetPendingLeaves()
        {
            try
            {
                var leaves = await _leaveService.GetPendingLeavesAsync();
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending leaves");
                return StatusCode(500, "An error occurred while retrieving pending leaves");
            }
        }

        // Apply for leave (Staff)
        [HttpPost("apply")]
        public async Task<ActionResult<LeaveResponse>> ApplyForLeave([FromBody] CreateLeaveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = Guid.Parse(GetCurrentUserId()); // ✅ Use UserId directly
                var leave = await _leaveService.CreateLeaveAsync(request, userId);

                return CreatedAtAction(nameof(GetLeave), new { id = leave.Id }, leave);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave application");
                return StatusCode(500, "An error occurred while creating the leave application");
            }
        }

        // Approve/Reject leave (Admin only)
        [HttpPost("approve")]
        public async Task<ActionResult<LeaveResponse>> ApproveLeave([FromBody] ApproveLeaveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = Guid.Parse(GetCurrentUserId());
                var leave = await _leaveService.ApproveLeaveAsync(request, adminId);

                return Ok(leave);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving/rejecting leave");
                return StatusCode(500, "An error occurred while processing the leave");
            }
        }

        // Upload attachment
        [HttpPost("{id}/attachments")]
        public async Task<ActionResult<LeaveAttachmentResponse>> UploadAttachment(
            Guid id,
            [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                var userId = Guid.Parse(GetCurrentUserId());
                var attachment = await _leaveService.UploadLeaveAttachmentAsync(id, file, userId);

                return Ok(attachment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading leave attachment");
                return StatusCode(500, "An error occurred while uploading the attachment");
            }
        }

        // Get remaining leave days
        [HttpGet("balance/{leaveType}")]
        public async Task<ActionResult<int>> GetLeaveBalance(LeaveType leaveType)
        {
            try
            {
                var userId = Guid.Parse(GetCurrentUserId()); // ✅ Use UserId directly
                var remainingDays = await _leaveService.GetRemainingLeaveDaysAsync(userId, leaveType);

                return Ok(remainingDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave balance");
                return StatusCode(500, "An error occurred while retrieving leave balance");
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value
                ?? throw new UnauthorizedAccessException("User role not found");
        }
    }
}
