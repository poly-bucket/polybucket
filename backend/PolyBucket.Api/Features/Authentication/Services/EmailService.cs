using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
using System;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger, PolyBucketDbContext context) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<EmailService> _logger = logger;
        private readonly PolyBucketDbContext _context = context;

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

        public async Task SendAdminCreatedAccountEmailAsync(string email, string username, string generatedPassword)
        {
            var subject = "Your PolyBucket Account Has Been Created";
            var body = $@"
                <h2>Welcome to PolyBucket!</h2>
                <p>Hi {username},</p>
                <p>An administrator has created a PolyBucket account for you.</p>
                <p><strong>Your login credentials:</strong></p>
                <ul>
                    <li><strong>Email:</strong> {email}</li>
                    <li><strong>Username:</strong> {username}</li>
                    <li><strong>Temporary Password:</strong> {generatedPassword}</li>
                </ul>
                <p><strong>Important Security Notice:</strong> Please log in and change your password immediately after your first login for security purposes.</p>
                <p>You can now start exploring and sharing your 3D models with the community.</p>
                <p>If you have any questions, feel free to reach out to our support team.</p>
                <br>
                <p>Best regards,<br>The PolyBucket Team</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> TestEmailConfigurationAsync(EmailSettings emailSettings, string testEmailAddress)
        {
            try
            {
                if (!emailSettings.IsValid())
                {
                    _logger.LogWarning("Email configuration is invalid for testing");
                    return false;
                }

                var subject = "PolyBucket Email Configuration Test";
                var body = @"
                    <h2>Email Configuration Test</h2>
                    <p>This is a test email to verify that your PolyBucket email configuration is working correctly.</p>
                    <p>If you received this email, your email settings are properly configured!</p>
                    <br>
                    <p>Best regards,<br>The PolyBucket Team</p>";

                await SendEmailAsync(testEmailAddress, subject, body, emailSettings);
                _logger.LogInformation("Test email sent successfully to {Email}", testEmailAddress);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", testEmailAddress);
                return false;
            }
        }

        public async Task<bool> IsEmailServiceConfiguredAsync()
        {
            try
            {
                var emailSettings = await GetEmailSettingsAsync();
                return emailSettings.Enabled && emailSettings.IsValid();
            }
            catch
            {
                return false;
            }
        }

        public async Task<EmailSettings> GetEmailSettingsAsync()
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Key.StartsWith("Email:"))
                .ToListAsync();

            var emailSettings = new EmailSettings();

            foreach (var setting in settings)
            {
                switch (setting.Key)
                {
                    case SystemSettingKeys.EmailEnabled:
                        emailSettings.Enabled = bool.TryParse(setting.Value, out var enabled) && enabled;
                        break;
                    case SystemSettingKeys.EmailSmtpServer:
                        emailSettings.SmtpServer = setting.Value ?? string.Empty;
                        break;
                    case SystemSettingKeys.EmailSmtpPort:
                        emailSettings.SmtpPort = int.TryParse(setting.Value, out var port) ? port : 587;
                        break;
                    case SystemSettingKeys.EmailSmtpUsername:
                        emailSettings.SmtpUsername = setting.Value ?? string.Empty;
                        break;
                    case SystemSettingKeys.EmailSmtpPassword:
                        emailSettings.SmtpPassword = setting.Value ?? string.Empty;
                        break;
                    case SystemSettingKeys.EmailUseSsl:
                        emailSettings.UseSsl = bool.TryParse(setting.Value, out var useSsl) && useSsl;
                        break;
                    case SystemSettingKeys.EmailFromAddress:
                        emailSettings.FromAddress = setting.Value ?? string.Empty;
                        break;
                    case SystemSettingKeys.EmailFromName:
                        emailSettings.FromName = setting.Value ?? string.Empty;
                        break;
                    case SystemSettingKeys.EmailRequireVerification:
                        emailSettings.RequireEmailVerification = bool.TryParse(setting.Value, out var requireVerification) && requireVerification;
                        break;
                }
            }

            return emailSettings;
        }

        private async Task SendEmailAsync(string to, string subject, string body, EmailSettings? customSettings = null)
        {
            try
            {
                var emailSettings = customSettings ?? await GetEmailSettingsAsync();

                if (!emailSettings.Enabled)
                {
                    _logger.LogWarning("Email service is disabled. Skipping email to {Email}", to);
                    return;
                }

                if (!emailSettings.IsValid())
                {
                    _logger.LogWarning("Email configuration is invalid. Cannot send email to {Email}", to);
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSettings.FromName, emailSettings.FromAddress));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                var secureSocketOptions = emailSettings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, secureSocketOptions);

                if (!string.IsNullOrEmpty(emailSettings.SmtpUsername) && !string.IsNullOrEmpty(emailSettings.SmtpPassword))
                {
                    await client.AuthenticateAsync(emailSettings.SmtpUsername, emailSettings.SmtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

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