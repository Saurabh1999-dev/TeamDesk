using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamDesk.DTOs;
using TeamDesk.DTOs.Request;
using TeamDesk.Models.Entities;

namespace TeamDesk.Services.Interfaces
{
    public interface IStaffService
    {
        Task<List<StaffResponse>> GetAllStaffAsync();
        Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request);
        Task<StaffResponse?> GetStaffByIdAsync(Guid id);
        Task<StaffResponse> UpdateStaffAsync(Guid id, UpdateStaffRequest request);
        Task<bool> DeleteStaffAsync(Guid id);
        Task<bool> StaffExistsAsync(string employeeCode, string email);
    }
}
