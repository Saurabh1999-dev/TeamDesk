using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class ProjectService : IprojectService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<Staff> _hasher = new();

        public ProjectService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<bool> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = new Project
            {
                ProjectName = dto.ProjectName,
                ClientName = dto.ClientName,
                LanguageUsed = dto.LanguageUsed,
                EndDate = dto.EndDate,
                IsBillable = dto.IsBillable
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<bool> UpdateProjectAsync(int id, CreateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            project.ProjectName = dto.ProjectName;
            project.ClientName = dto.ClientName;
            project.LanguageUsed = dto.LanguageUsed;
            project.EndDate = dto.EndDate;
            project.IsBillable = dto.IsBillable;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
