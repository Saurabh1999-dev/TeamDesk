// Controllers/StaffController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active staff members
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<StaffResponse>>> GetAllStaff()
        {
            try
            {
                var staff = await _staffService.GetAllStaffAsync();
                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff members");
                return StatusCode(500, new { message = "An error occurred while retrieving staff members" });
            }
        }

        /// <summary>
        /// Get staff member by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<StaffResponse>> GetStaffById(Guid id)
        {
            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id);

                if (staff == null)
                {
                    return NotFound(new { message = $"Staff member with ID {id} not found" });
                }

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff member with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving staff member" });
            }
        }

        /// <summary>
        /// Create new staff member (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<StaffResponse>> CreateStaff([FromBody] CreateStaffRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var staff = await _staffService.CreateStaffAsync(request);
                return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id }, staff);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff member");
                return StatusCode(500, new { message = "An error occurred while creating staff member" });
            }
        }

        /// <summary>
        /// Update existing staff member
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<StaffResponse>> UpdateStaff(Guid id, [FromBody] UpdateStaffRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var staff = await _staffService.UpdateStaffAsync(id, request);
                return Ok(staff);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff member with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating staff member" });
            }
        }

        /// <summary>
        /// Delete staff member (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteStaff(Guid id)
        {
            try
            {
                var result = await _staffService.DeleteStaffAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Staff member with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff member with ID {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting staff member" });
            }
        }
    }
}
