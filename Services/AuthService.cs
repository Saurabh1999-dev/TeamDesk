using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly PasswordHasher<User> _hasher = new();
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, IConfiguration config, JwtService jwtService)
        {
            _context = context;
            _config = config;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var normalizedEmail = request.Email.Trim().ToLower();

                var user = await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                var token = _jwtService.GenerateToken(user);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role
                    }
                };
            }
            catch (Exception)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during loginnnnnnnnnnnnnnnnnnnnn............................................................"
                };
            }
        }



        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    };
                }

                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = request.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);
                return new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            return await _context.User.FirstOrDefaultAsync(s => s.Email == email);
        }
    }

}
