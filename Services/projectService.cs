// Services/ProjectService.cs
using Microsoft.EntityFrameworkCore;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.Enum;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(AppDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProjectResponse>> GetAllProjectsAsync()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.StaffAssignments)
                        .ThenInclude(psa => psa.Staff)
                        .ThenInclude(s => s.User)
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return projects.Select(MapToProjectResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                throw;
            }
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.StaffAssignments)
                        .ThenInclude(psa => psa.Staff)
                        .ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                return project != null ? MapToProjectResponse(project) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID {ProjectId}", id);
                throw;
            }
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request)
        {
            try
            {
                // Validate client exists
                var client = await _context.Clients.FindAsync(request.ClientId);
                if (client == null)
                {
                    throw new InvalidOperationException($"Client with ID {request.ClientId} not found");
                }

                var project = new Project
                {
                    Name = request.Name,
                    Description = request.Description,
                    ClientId = request.ClientId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Deadline = request.Deadline,
                    Budget = request.Budget,
                    Priority = request.Priority,
                    Status = ProjectStatus.Planning,
                    Progress = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Add staff assignments if provided
                if (request.StaffAssignments.Any())
                {
                    foreach (var assignment in request.StaffAssignments)
                    {
                        await AssignStaffToProjectInternalAsync(project.Id, assignment);
                    }
                }

                // Reload project with related data
                var createdProject = await GetProjectByIdAsync(project.Id);
                _logger.LogInformation("Created new project: {ProjectName} with ID {ProjectId}",
                    project.Name, project.Id);

                return createdProject!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                throw;
            }
        }

        public async Task<ProjectResponse> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null || !project.IsActive)
                {
                    throw new InvalidOperationException($"Project with ID {id} not found");
                }

                // Update project properties
                project.Name = request.Name;
                project.Description = request.Description;
                project.EndDate = request.EndDate;
                project.Deadline = request.Deadline;
                project.Budget = request.Budget;
                project.Status = request.Status;
                project.Priority = request.Priority;
                project.Progress = request.Progress;
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedProject = await GetProjectByIdAsync(id);
                _logger.LogInformation("Updated project: {ProjectName} with ID {ProjectId}",
                    project.Name, project.Id);

                return updatedProject!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with ID {ProjectId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null) return false;

                // Soft delete
                project.IsActive = false;
                project.UpdatedAt = DateTime.UtcNow;

                // Deactivate staff assignments
                var assignments = await _context.ProjectStaffAssignments
                    .Where(psa => psa.ProjectId == id && psa.IsActive)
                    .ToListAsync();

                foreach (var assignment in assignments)
                {
                    assignment.IsActive = false;
                    assignment.UnassignedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Soft deleted project with ID {ProjectId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID {ProjectId}", id);
                throw;
            }
        }

        public async Task<List<ProjectResponse>> GetProjectsByStatusAsync(string status)
        {
            try
            {
                if (!System.Enum.TryParse<ProjectStatus>(status, true, out var projectStatus))
                {
                    throw new ArgumentException($"Invalid project status: {status}");
                }

                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.StaffAssignments)
                        .ThenInclude(psa => psa.Staff)
                        .ThenInclude(s => s.User)
                    .Where(p => p.IsActive && p.Status == projectStatus)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return projects.Select(MapToProjectResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects by status {Status}", status);
                throw;
            }
        }

        public async Task<List<ProjectResponse>> GetOverdueProjectsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.StaffAssignments)
                        .ThenInclude(psa => psa.Staff)
                        .ThenInclude(s => s.User)
                    .Where(p => p.IsActive &&
                               p.Status != ProjectStatus.Completed &&
                               p.Deadline.Date < today)
                    .OrderBy(p => p.Deadline)
                    .ToListAsync();

                return projects.Select(MapToProjectResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue projects");
                throw;
            }
        }

        public async Task<ProjectResponse> AssignStaffToProjectAsync(Guid projectId, ProjectStaffAssignmentRequest request)
        {
            try
            {
                await AssignStaffToProjectInternalAsync(projectId, request);
                var updatedProject = await GetProjectByIdAsync(projectId);
                return updatedProject!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to project");
                throw;
            }
        }

        public async Task<bool> RemoveStaffFromProjectAsync(Guid projectId, Guid staffId)
        {
            try
            {
                var assignment = await _context.ProjectStaffAssignments
                    .FirstOrDefaultAsync(psa => psa.ProjectId == projectId &&
                                               psa.StaffId == staffId &&
                                               psa.IsActive);

                if (assignment == null) return false;

                assignment.IsActive = false;
                assignment.UnassignedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed staff {StaffId} from project {ProjectId}",
                    staffId, projectId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing staff from project");
                throw;
            }
        }

        public async Task<List<ClientResponse>> GetAllClientsAsync()
        {
            try
            {
                var clients = await _context.Clients
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return clients.Select(c => new ClientResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactPerson = c.ContactPerson,
                    ContactEmail = c.ContactEmail,
                    ContactPhone = c.ContactPhone
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                throw;
            }
        }

        public async Task<ClientResponse> CreateClientAsync(CreateClientRequest request)
        {
            try
            {
                // Check if client with same email exists
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.ContactEmail == request.ContactEmail);

                if (existingClient != null)
                {
                    throw new InvalidOperationException($"Client with email {request.ContactEmail} already exists");
                }

                var client = new Client
                {
                    Name = request.Name,
                    Description = request.Description,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    ContactPerson = request.ContactPerson,
                    Address = request.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new client: {ClientName} with ID {ClientId}",
                    client.Name, client.Id);

                return new ClientResponse
                {
                    Id = client.Id,
                    Name = client.Name,
                    ContactPerson = client.ContactPerson,
                    ContactEmail = client.ContactEmail,
                    ContactPhone = client.ContactPhone
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                throw;
            }
        }

        // Private helper methods
        private async Task AssignStaffToProjectInternalAsync(Guid projectId, ProjectStaffAssignmentRequest request)
        {
            // Validate staff exists
            var staff = await _context.Staff.FindAsync(request.StaffId);
            if (staff == null)
            {
                throw new InvalidOperationException($"Staff with ID {request.StaffId} not found");
            }

            // Check if staff is already assigned to this project
            var existingAssignment = await _context.ProjectStaffAssignments
                .FirstOrDefaultAsync(psa => psa.ProjectId == projectId &&
                                           psa.StaffId == request.StaffId &&
                                           psa.IsActive);

            if (existingAssignment != null)
            {
                throw new InvalidOperationException($"Staff is already assigned to this project");
            }

            var assignment = new ProjectStaffAssignment
            {
                ProjectId = projectId,
                StaffId = request.StaffId,
                Role = request.Role,
                AllocationPercentage = request.AllocationPercentage,
                AssignedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.ProjectStaffAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        private static ProjectResponse MapToProjectResponse(Project project)
        {
            var today = DateTime.UtcNow.Date;
            var daysRemaining = (project.Deadline.Date - today).Days;

            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Client = new ClientResponse
                {
                    Id = project.Client.Id,
                    Name = project.Client.Name,
                    ContactPerson = project.Client.ContactPerson,
                    ContactEmail = project.Client.ContactEmail,
                    ContactPhone = project.Client.ContactPhone
                },
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Deadline = project.Deadline,
                Budget = project.Budget,
                Status = project.Status.ToString(),
                Priority = project.Priority.ToString(),
                Progress = project.Progress,
                IsOverdue = project.Deadline.Date < today && project.Status != ProjectStatus.Completed,
                DaysRemaining = daysRemaining,
                StaffAssignments = project.StaffAssignments
                    .Where(psa => psa.IsActive)
                    .Select(psa => new ProjectStaffAssignmentResponse
                    {
                        Id = psa.Id,
                        StaffId = psa.StaffId,
                        StaffName = $"{psa.Staff.User.FirstName} {psa.Staff.User.LastName}",
                        StaffEmail = psa.Staff.User.Email,
                        Role = psa.Role.ToString(),
                        AllocationPercentage = psa.AllocationPercentage,
                        AssignedDate = psa.AssignedDate,
                        IsActive = psa.IsActive
                    }).ToList(),
                CreatedAt = project.CreatedAt
            };
        }
    }
}
