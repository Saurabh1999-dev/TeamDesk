// Services/EmailService.cs - Complete Implementation
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        // ✅ Base email sending method that your password reset methods call
        public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = _emailSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                return false;
            }
        }

        // ✅ Your password reset email methods (fixed)
        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string userName)
        {
            try
            {
                var subject = "Password Reset Request - TeamDesk";

                var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #333; text-align: center;'>Password Reset Request</h2>
                        
                        <p>Hello {userName},</p>
                        
                        <p>You recently requested to reset your password for your TeamDesk account. Click the button below to reset it:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Reset My Password
                            </a>
                        </div>
                        
                        <p><strong>Important:</strong> This link will expire in 1 hour for security reasons.</p>
                        
                        <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
                        
                        <hr style='border: none; height: 1px; background-color: #eee; margin: 20px 0;'>
                        
                        <p style='font-size: 12px; color: #666;'>
                            If the button doesn't work, copy and paste this link into your browser:<br>
                            <a href='{resetLink}'>{resetLink}</a>
                        </p>
                        
                        <p style='font-size: 12px; color: #666;'>
                            Best regards,<br>
                            The TeamDesk Team
                        </p>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetConfirmationEmailAsync(string email, string userName)
        {
            try
            {
                var subject = "Password Successfully Reset - TeamDesk";

                var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #28a745; text-align: center;'>Password Successfully Reset</h2>
                        
                        <p>Hello {userName},</p>
                        
                        <p>Your password has been successfully reset for your TeamDesk account.</p>
                        
                        <p>If you did not make this change, please contact our support team immediately.</p>
                        
                        <p style='font-size: 12px; color: #666;'>
                            Best regards,<br>
                            The TeamDesk Team
                        </p>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset confirmation email to {Email}", email);
                return false;
            }
        }

        // ✅ Task assignment email method (if you need this from previous conversations)
        public async Task<bool> SendTaskAssignmentEmailAsync(string email, string userName, string taskTitle, string projectName, string? customMessage)
        {
            try
            {
                var subject = $"New Task Assigned: {taskTitle}";

                var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #007bff; text-align: center;'>New Task Assigned</h2>
                        
                        <p>Hello {userName},</p>
                        
                        <p>You have been assigned a new task:</p>
                        
                        <div style='background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #007bff; border-radius: 5px;'>
                            <h3 style='margin: 0 0 10px 0; color: #333;'>{taskTitle}</h3>
                            <p style='margin: 0; color: #666;'><strong>Project:</strong> {projectName}</p>
                            {(string.IsNullOrEmpty(customMessage) ? "" : $"<p style='margin: 10px 0 0 0; color: #333;'>{customMessage}</p>")}
                        </div>
                        
                        <p>Please log in to your TeamDesk account to view the full task details and get started.</p>
                        
                        <p style='font-size: 12px; color: #666;'>
                            Best regards,<br>
                            The TeamDesk Team
                        </p>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending task assignment email to {Email}", email);
                return false;
            }
        }
    }
}
