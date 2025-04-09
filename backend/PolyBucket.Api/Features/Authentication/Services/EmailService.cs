using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
        {
            var subject = "Reset Your PolyBucket Password";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password for your PolyBucket account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetUrl}?token={resetToken}'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>This link will expire in 1 hour.</p>
                <br>
                <p>Best regards,<br>The PolyBucket Team</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailVerificationAsync(string email, string verificationToken, string verificationUrl)
        {
            var subject = "Verify Your PolyBucket Email";
            var body = $@"
                <h2>Email Verification</h2>
                <p>Welcome to PolyBucket! Please verify your email address to complete your registration.</p>
                <p>Click the link below to verify your email:</p>
                <p><a href='{verificationUrl}?token={verificationToken}'>Verify Email</a></p>
                <p>If you didn't create an account, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
                <br>
                <p>Best regards,<br>The PolyBucket Team</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string username)
        {
            var subject = "Welcome to PolyBucket!";
            var body = $@"
                <h2>Welcome to PolyBucket!</h2>
                <p>Hi {username},</p>
                <p>Thank you for joining PolyBucket! Your account has been successfully created.</p>
                <p>You can now start uploading and sharing your 3D models with the community.</p>
                <p>If you have any questions, feel free to reach out to our support team.</p>
                <br>
                <p>Best regards,<br>The PolyBucket Team</p>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["AppSettings:Email:SmtpServer"];
                var smtpPort = Convert.ToInt32(_configuration["AppSettings:Email:SmtpPort"]);
                var smtpUsername = _configuration["AppSettings:Email:SmtpUsername"];
                var smtpPassword = _configuration["AppSettings:Email:SmtpPassword"];
                var useSsl = Convert.ToBoolean(_configuration["AppSettings:Email:UseSsl"]);
                var fromEmail = _configuration["AppSettings:Email:FromEmail"];
                var fromName = _configuration["AppSettings:Email:FromName"];

                if (string.IsNullOrEmpty(smtpServer))
                {
                    _logger.LogWarning("SMTP server not configured. Email sending is disabled.");
                    return;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = useSsl,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }
    }
} 