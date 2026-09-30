using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Users.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Register.Domain
{
    public class RegisterCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IEmailSettingsResolver emailSettingsResolver,
        IAccountEmailService accountEmailService,
        IPasswordHasher passwordHasher,
        ILogger<RegisterCommandHandler> logger,
        PolyBucketDbContext context)
    {
        public static readonly TimeSpan EmailVerificationLifetime = TimeSpan.FromHours(24);

        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;
        private readonly IAccountEmailService _accountEmailService = accountEmailService;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
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

            await _authRepository.CreateRefreshTokenAsync(new PolyBucket.Api.Features.Authentication.Domain.RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = authResponse.RefreshToken,
                UserId = user.Id,
                ExpiresAt = authResponse.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = command.Client.IpAddress
            });

            // Create login record
            var loginRecord = new UserLogin
            {
                Id = Guid.NewGuid(),
                Email = user.Email,
                UserId = user.Id,
                Successful = true,
                IpAddress = command.Client.IpAddress,
                UserAgent = command.Client.UserAgent,
                CreatedAt = DateTime.UtcNow
            };
            await _authRepository.CreateLoginRecordAsync(loginRecord);

            var emailSettings = await _emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
            var isEmailServiceConfigured = emailSettings.CanDeliver;
            var requiresEmailVerification = isEmailServiceConfigured && emailSettings.RequireEmailVerification;

            if (isEmailServiceConfigured)
            {
                if (requiresEmailVerification)
                {
                    var emailVerificationToken = _tokenService.GenerateEmailVerificationToken();
                    var verificationToken = new EmailVerificationToken
                    {
                        Id = Guid.NewGuid(),
                        Token = TokenHasher.Hash(emailVerificationToken),
                        Email = user.Email,
                        UserId = user.Id,
                        Purpose = EmailVerificationPurpose.VerifyAddress,
                        ExpiresAt = DateTime.UtcNow.Add(EmailVerificationLifetime),
                        CreatedAt = DateTime.UtcNow,
                        CreatedByIp = command.Client.IpAddress
                    };

                    await _authRepository.CreateEmailVerificationTokenAsync(verificationToken);
                    await _accountEmailService.SendVerificationAsync(user, emailVerificationToken, EmailVerificationLifetime, cancellationToken: cancellationToken);

                    _logger.LogInformation("Email verification queued for user: {Email}", user.Email);
                }
                else
                {
                    await _accountEmailService.SendWelcomeAsync(user, cancellationToken: cancellationToken);
                    _logger.LogInformation("Welcome email queued for user: {Email}", user.Email);
                }
            }
            else
            {
                _logger.LogWarning("Email service is not configured. Skipping email sending for user: {Email}", user.Email);
            }

            return new RegisterCommandResponse
            {
                Authentication = authResponse,
                RequiresEmailVerification = requiresEmailVerification
            };
        }
    }
} 