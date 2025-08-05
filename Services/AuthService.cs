using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<Staff> _hasher = new();

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == request.Email);
                if (staff == null) throw new Exception("Invalid credentials");

                var result = _hasher.VerifyHashedPassword(staff, staff.PasswordHash, request.Password);
                if (result == PasswordVerificationResult.Failed)
                    throw new Exception("Invalid credentials");

                //var token = JwtTokenGenerator.GenerateToken(staff, _config);
                return new AuthResponse
                {
                    StaffId = staff.StaffId,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    //Role = staff.Role,
                    //Token = token
                };
            }catch(Exception e)
            {
                throw new Exception();
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Staff.AnyAsync(s => s.Email == request.Email))
                throw new Exception("Email already exists");

            var staff = new Staff
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role.ToString(),
            };
            staff.PasswordHash = _hasher.HashPassword(staff, request.Password);

            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();
            return new AuthResponse
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                Email = staff.Email,
                //Role = staff.Role,
            };
        }

        public async Task<Staff> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            return await _context.Staff.FirstOrDefaultAsync(s => s.Email == email);
        }
    }

}
