using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.CreateUser.Domain
{
    public class CreateUserCommandHandler(
        IAuthenticationRepository authRepository,
        IPasswordHasher passwordHasher,
        IPasswordGenerator passwordGenerator,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<CreateUserCommandHandler> logger,
        PolyBucketDbContext context)
    {
        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IPasswordGenerator _passwordGenerator = passwordGenerator;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<CreateUserCommandHandler> _logger = logger;
        private readonly PolyBucketDbContext _context = context;

        public async Task<CreateUserCommandResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
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

            // Generate secure password
            var generatedPassword = _passwordGenerator.GeneratePassword();

            // Find the role
            var role = await _context.Roles.FindAsync(command.RoleId);
            if (role == null)
            {
                throw new ArgumentException($"Role with ID {command.RoleId} not found.", nameof(command.RoleId));
            }

            // Hash password
            var salt = _passwordHasher.GenerateSalt();
            var passwordHash = _passwordHasher.HashPassword(generatedPassword, salt);

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
                RoleId = command.RoleId,
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

            // Log successful user creation
            _logger.LogInformation("User created by admin: {Email} with role {Role}", user.Email, user.Role);

            // Create login record for admin creation
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

            if (requiresEmailVerification)
            {
                // Send welcome email with password (for admin-created accounts, we send the password directly)
                await _emailService.SendAdminCreatedAccountEmailAsync(user.Email, user.Username, generatedPassword);
            }
            else
            {
                // Send welcome email with password
                await _emailService.SendAdminCreatedAccountEmailAsync(user.Email, user.Username, generatedPassword);
            }

            return new CreateUserCommandResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                RoleId = user.RoleId ?? Guid.Empty,
                RoleName = role.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Country = user.Country,
                GeneratedPassword = generatedPassword,
                CreatedAt = user.CreatedAt,
                EmailVerificationRequired = requiresEmailVerification
            };
        }
    }
} 