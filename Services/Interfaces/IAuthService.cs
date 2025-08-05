using System.Security.Claims;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;

namespace TeamDesk.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<User> GetCurrentUserAsync(ClaimsPrincipal user);
    }
}
