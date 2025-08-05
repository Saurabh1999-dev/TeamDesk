using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpGet("verify")]
        public async Task<ActionResult<AuthResponse>> VerifyToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Token not provided"
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                }

                // Find user in database
                var user = await _context.User.FindAsync(Guid.Parse(userId));
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "User not found or inactive"
                    });
                }

                // Return user data if token is valid
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Token is valid",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role
                    }
                });
            }
            catch
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }
        }
    }

}
