using System.Threading.Tasks;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
        Task SendEmailVerificationAsync(string email, string verificationToken, string verificationUrl);
        Task SendWelcomeEmailAsync(string email, string username);
        Task SendAdminCreatedAccountEmailAsync(string email, string username, string generatedPassword);
        
        // Configuration management methods
        Task<bool> TestEmailConfigurationAsync(EmailSettings emailSettings, string testEmailAddress);
        Task<bool> IsEmailServiceConfiguredAsync();
        Task<EmailSettings> GetEmailSettingsAsync();
    }
} 