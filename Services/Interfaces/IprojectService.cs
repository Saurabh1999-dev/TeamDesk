using System.Security.Claims;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;

namespace TeamDesk.Services.Interfaces
{
    public interface IprojectService
    {
        Task<bool> CreateProjectAsync(CreateProjectDto dto);
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project> GetProjectByIdAsync(int id);
        Task<bool> UpdateProjectAsync(int id, CreateProjectDto dto);
        Task<bool> DeleteProjectAsync(int id);
    }

}
