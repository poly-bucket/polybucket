using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.CreateUser.Repository;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.CreateUser.Domain;

public class CreateUserService(
    IAuthenticationRepository authRepository,
    ICreateUserRepository createUserRepository,
    IPasswordHasher passwordHasher,
    IPasswordGenerator passwordGenerator,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<CreateUserService> logger) : ICreateUserService
{
    public async Task<CreateUserCommandResponse> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (await authRepository.IsEmailTakenAsync(command.Email))
        {
            throw new InvalidOperationException("Email is already registered");
        }

        if (await authRepository.IsUsernameTakenAsync(command.Username))
        {
            throw new InvalidOperationException("Username is already taken");
        }

        var generatedPassword = passwordGenerator.GeneratePassword();

        var role = await createUserRepository.GetRoleByIdAsync(command.RoleId, cancellationToken);
        if (role == null)
        {
            throw new ArgumentException($"Role with ID {command.RoleId} not found.", nameof(command.RoleId));
        }

        var salt = passwordHasher.GenerateSalt();
        var passwordHash = passwordHasher.HashPassword(generatedPassword, salt);

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
                UserId = Guid.NewGuid(),
                Language = "en",
                Theme = "dark",
                EmailNotifications = true,
                MeasurementSystem = "metric",
                TimeZone = "UTC"
            }
        };

        user.Settings.UserId = user.Id;

        await authRepository.CreateUserAsync(user);

        logger.LogInformation("User created by admin: {Email} with role {Role}", user.Email, user.Role);

        var loginRecord = new UserLogin
        {
            Id = Guid.NewGuid(),
            Email = user.Email,
            UserId = user.Id,
            Successful = true,
            UserAgent = command.UserAgent,
            CreatedAt = DateTime.UtcNow
        };
        await authRepository.CreateLoginRecordAsync(loginRecord);

        var requiresEmailVerification = Convert.ToBoolean(configuration["AppSettings:Email:RequireEmailVerification"] ?? "false");

        if (requiresEmailVerification)
        {
            await emailService.SendAdminCreatedAccountEmailAsync(user.Email, user.Username, generatedPassword);
        }
        else
        {
            await emailService.SendAdminCreatedAccountEmailAsync(user.Email, user.Username, generatedPassword);
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
