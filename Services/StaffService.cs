using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class StaffService : IStaffService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StaffService> _logger;

        public StaffService(AppDbContext context, ILogger<StaffService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<StaffResponse>> GetAllStaffAsync()
        {
            try
            {
                var staffEntities = await _context.Staff
                    .Include(s => s.User)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.User.FirstName)
                    .ToListAsync();

                var staff = staffEntities.Select(s => MapToStaffResponse(s)).ToList();

                _logger.LogInformation("Retrieved {Count} staff members", staff.Count);
                return staff;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff members");
                throw; // Let controller handle the exception
            }
        }

        public async Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request)
        {
            try
            {
                // Validate if staff already exists
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email {request.Email} already exists");
                }

                var existingStaff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.EmployeeCode == request.EmployeeCode);

                if (existingStaff != null)
                {
                    throw new InvalidOperationException($"Staff with employee code {request.EmployeeCode} already exists");
                }

                // Create user first
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TempPassword123!"),
                    Role = request.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                // Create staff record
                var staff = new Staff
                {
                    UserId = user.Id,
                    EmployeeCode = request.EmployeeCode,
                    Department = request.Department,
                    Position = request.Position,
                    HireDate = request.HireDate,
                    Salary = request.Salary,
                    Skills = JsonSerializer.Serialize(request.Skills ?? new List<string>()),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();

                // Load the created staff with user data
                var createdStaff = await _context.Staff
                    .Include(s => s.User)
                    .FirstAsync(s => s.Id == staff.Id);

                var response = MapToStaffResponse(createdStaff);
                _logger.LogInformation("Created new staff member: {EmployeeCode} - {FullName}",
                    response.EmployeeCode, response.FullName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff member with email {Email}", request.Email);
                throw;
            }
        }

        public async Task<StaffResponse?> GetStaffByIdAsync(Guid id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                return staff != null ? MapToStaffResponse(staff) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff member with ID {Id}", id);
                throw;
            }
        }

        public async Task<StaffResponse> UpdateStaffAsync(Guid id, UpdateStaffRequest request)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (staff == null)
                {
                    throw new InvalidOperationException($"Staff member with ID {id} not found");
                }

                // Update staff properties
                staff.Department = request.Department;
                staff.Position = request.Position;
                staff.Salary = request.Salary;
                staff.Skills = JsonSerializer.Serialize(request.Skills ?? new List<string>());
                staff.UpdatedAt = DateTime.UtcNow;

                // Update user properties if provided
                if (!string.IsNullOrEmpty(request.FirstName))
                    staff.User.FirstName = request.FirstName;

                if (!string.IsNullOrEmpty(request.LastName))
                    staff.User.LastName = request.LastName;

                staff.User.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = MapToStaffResponse(staff);
                _logger.LogInformation("Updated staff member: {EmployeeCode}", response.EmployeeCode);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff member with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStaffAsync(Guid id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null) return false;

                // Soft delete
                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted staff member with ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff member with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> StaffExistsAsync(string employeeCode, string email)
        {
            return await _context.Staff
                .Include(s => s.User)
                .AnyAsync(s => s.EmployeeCode == employeeCode || s.User.Email == email);
        }

        // Private helper method for mapping
        private static StaffResponse MapToStaffResponse(Staff staff)
        {
            return new StaffResponse
            {
                Id = staff.Id,
                FullName = $"{staff.User.FirstName} {staff.User.LastName}",
                Email = staff.User.Email,
                EmployeeCode = staff.EmployeeCode,
                Department = staff.Department,
                Position = staff.Position,
                HireDate = staff.HireDate,
                Skills = string.IsNullOrEmpty(staff.Skills)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(staff.Skills) ?? new List<string>(),
                IsActive = staff.IsActive,
                CreatedAt = staff.CreatedAt
            };
        }
    }
}
