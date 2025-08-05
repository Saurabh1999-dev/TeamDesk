using TeamDesk.DTOs;

namespace TeamDesk.Services.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectResponse>> GetAllProjectsAsync();
        Task<ProjectResponse?> GetProjectByIdAsync(Guid id);
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
        Task<ProjectResponse> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
        Task<bool> DeleteProjectAsync(Guid id);
        Task<List<ProjectResponse>> GetProjectsByStatusAsync(string status);
        Task<List<ProjectResponse>> GetOverdueProjectsAsync();
        Task<ProjectResponse> AssignStaffToProjectAsync(Guid projectId, ProjectStaffAssignmentRequest request);
        Task<bool> RemoveStaffFromProjectAsync(Guid projectId, Guid staffId);
        Task<List<ClientResponse>> GetAllClientsAsync();
        Task<ClientResponse> CreateClientAsync(CreateClientRequest request);
    }
}
