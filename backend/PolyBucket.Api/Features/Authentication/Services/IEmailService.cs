using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
        Task SendEmailVerificationAsync(string email, string verificationToken, string verificationUrl);
        Task SendWelcomeEmailAsync(string email, string username);
    }
} 