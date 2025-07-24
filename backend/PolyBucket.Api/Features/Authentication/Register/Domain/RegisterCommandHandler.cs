using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Register.Domain
{
    public class RegisterCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger,
        PolyBucketDbContext context)
    {
        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailService _emailService = emailService;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<RegisterCommandHandler> _logger = logger;
        private readonly PolyBucketDbContext _context = context;

        public async Task<RegisterCommandResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            // Check if email is already taken
            if (await _authRepository.IsEmailTakenAsync(command.Email))
            {
                throw new InvalidOperationException("Email is already registered");
            }

            // Check if username is already taken
            if (await _authRepository.IsUsernameTakenAsync(command.Username))
            {
                throw new InvalidOperationException("Username is already taken");
            }

            // Hash password
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, salt);

            // Find the default User role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                throw new InvalidOperationException("Default User role not found. Please ensure roles are properly configured.");
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Country = command.Country,
                PasswordHash = passwordHash,
                Salt = salt,
                RoleId = userRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Settings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(), // Will be set after user creation
                    Language = "en",
                    Theme = "dark",
                    EmailNotifications = true,
                    MeasurementSystem = "metric",
                    TimeZone = "UTC"
                }
            };

            // Set the UserId for settings
            user.Settings.UserId = user.Id;

            // Save user
            await _authRepository.CreateUserAsync(user);

            // Log successful registration
            _logger.LogInformation("User registered successfully: {Email}", user.Email);

            // Generate authentication response
            var authResponse = _tokenService.GenerateAuthenticationResponse(user);

            // Create login record
            var loginRecord = new UserLogin
            {
                Id = Guid.NewGuid(),
                Email = user.Email,
                UserId = user.Id,
                Successful = true,
                UserAgent = command.UserAgent,
                CreatedAt = DateTime.UtcNow
            };
            await _authRepository.CreateLoginRecordAsync(loginRecord);

            // Check if email service is configured and if email verification is required
            var isEmailServiceConfigured = await _emailService.IsEmailServiceConfiguredAsync();
            var emailSettings = await _emailService.GetEmailSettingsAsync();
            var requiresEmailVerification = isEmailServiceConfigured && emailSettings.RequireEmailVerification;
            string? emailVerificationToken = null;

            if (isEmailServiceConfigured)
            {
                if (requiresEmailVerification)
                {
                    emailVerificationToken = _tokenService.GenerateEmailVerificationToken();
                    var verificationToken = new EmailVerificationToken
                    {
                        Id = Guid.NewGuid(),
                        Token = emailVerificationToken,
                        Email = user.Email,
                        ExpiresAt = DateTime.UtcNow.AddHours(24),
                        CreatedAt = DateTime.UtcNow,
                        CreatedByIp = "127.0.0.1" // TODO: Get from request
                    };

                    await _authRepository.CreateEmailVerificationTokenAsync(verificationToken);

                    // Send verification email
                    var frontendUrl = _configuration["AppSettings:Frontend:BaseUrl"];
                    var verificationUrl = $"{frontendUrl}/verify-email";
                    await _emailService.SendEmailVerificationAsync(user.Email, emailVerificationToken, verificationUrl);
                    
                    _logger.LogInformation("Email verification sent to user: {Email}", user.Email);
                }
                else
                {
                    // Send welcome email if email service is configured but verification is not required
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);
                    _logger.LogInformation("Welcome email sent to user: {Email}", user.Email);
                }
            }
            else
            {
                // Email service is not configured, skip email sending
                _logger.LogWarning("Email service is not configured. Skipping email sending for user: {Email}", user.Email);
            }

            return new RegisterCommandResponse
            {
                Authentication = authResponse,
                RequiresEmailVerification = requiresEmailVerification,
                EmailVerificationToken = emailVerificationToken
            };
        }
    }
} 