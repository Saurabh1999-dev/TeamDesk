using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetController> _logger;
        private readonly AppDbContext _context;

        public PasswordResetController(
            IEmailService emailService,
            ILogger<PasswordResetController> logger,
            AppDbContext context
        )
        {
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Step 1: Request password reset link
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Please provide a valid email address."
                });
            }

            var normalizedEmail = request.Email.Trim().ToLower();

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);
            var response = new ForgotPasswordResponse
            {
                Success = true,
                Message = "If an account with that email exists, a password reset link has been sent."
            };

            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return Ok(response);
            }

            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // long random token
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _context.Update(user);
            await _context.SaveChangesAsync();
            var resetLink = $"{request.ClientURI}/?email={Uri.EscapeDataString(user.Email)}&token={encodedToken}";

            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                resetLink,
                $"{user.FirstName} {user.LastName}"
            );

            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to {Email}", user.Email);
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Failed to send reset email. Please try again later."
                });
            }

            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            return Ok(response);
        }

        /// <summary>
        /// Step 2: Verify reset token
        /// </summary>
        [HttpPost("verify-reset-token")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new VerifyTokenResponse
                {
                    IsValid = false,
                    Message = "Invalid request parameters."
                });
            }

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                return Ok(new VerifyTokenResponse
                {
                    IsValid = false,
                    Message = "Invalid reset token."
                });
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            }
            catch
            {
                return Ok(new VerifyTokenResponse
                {
                    IsValid = false,
                    Message = "Invalid reset token format."
                });
            }

            if (user.PasswordResetToken != decodedToken || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return Ok(new VerifyTokenResponse
                {
                    IsValid = false,
                    Message = "Invalid or expired reset token."
                });
            }

            return Ok(new VerifyTokenResponse
            {
                IsValid = true,
                Email = user.Email,
                Message = "Token is valid."
            });
        }

        /// <summary>
        /// Step 3: Reset password
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid input."
                });
            }

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                return BadRequest(new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid reset token."
                });
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            }
            catch
            {
                return BadRequest(new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid reset token format."
                });
            }

            if (user.PasswordResetToken != decodedToken || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid or expired reset token."
                });
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Update(user);
            await _context.SaveChangesAsync();
            await _emailService.SendPasswordResetConfirmationEmailAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}"
            );

            _logger.LogInformation("Password successfully reset for user {Email}", user.Email);

            return Ok(new ResetPasswordResponse
            {
                Success = true,
                Message = "Password has been successfully reset."
            });
        }
    }
}
