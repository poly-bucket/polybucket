using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Enums;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Register.Domain
{
    public class RegisterCommandHandler
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IAuthenticationRepository authRepository,
            ITokenService tokenService,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ILogger<RegisterCommandHandler> logger)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
        }

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
                Role = UserRole.User,
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

            // Check if email verification is required
            var requiresEmailVerification = Convert.ToBoolean(_configuration["AppSettings:Email:RequireEmailVerification"] ?? "false");
            string? emailVerificationToken = null;

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
            }
            else
            {
                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);
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