// Services/Interfaces/IEmailService.cs
namespace TeamDesk.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string htmlMessage);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string userName);
        Task<bool> SendPasswordResetConfirmationEmailAsync(string email, string userName);
        Task<bool> SendTaskAssignmentEmailAsync(string email, string userName, string taskTitle, string projectName, string? customMessage);
    }
}
