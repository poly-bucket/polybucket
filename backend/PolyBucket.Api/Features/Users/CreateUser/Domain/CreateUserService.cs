using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Users.CreateUser.Repository;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.CreateUser.Domain;

public class CreateUserService(
    IAuthenticationRepository authRepository,
    ICreateUserRepository createUserRepository,
    IPasswordHasher passwordHasher,
    IPasswordGenerator passwordGenerator,
    ITokenService tokenService,
    IEmailSettingsResolver emailSettingsResolver,
    IAccountEmailService accountEmailService,
    ILogger<CreateUserService> logger) : ICreateUserService
{
    public static readonly TimeSpan InviteLifetime = TimeSpan.FromHours(72);

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
            EmailVerifiedAt = command.MarkEmailVerified ? DateTime.UtcNow : null,
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

        logger.LogInformation("User created by admin: {Email} with role {Role}", user.Email, role.Name);

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

        var emailSettings = await emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var inviteQueued = false;

        if (emailSettings.CanDeliver && emailSettings.HasPublicBaseUrl)
        {
            var inviteToken = tokenService.GeneratePasswordResetToken();
            await authRepository.CreatePasswordResetTokenAsync(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash(inviteToken),
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.Add(InviteLifetime),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ClientRequestInfo.UnknownIp
            });

            var outcome = await accountEmailService.SendAccountInviteAsync(user, inviteToken, InviteLifetime, cancellationToken: cancellationToken);
            inviteQueued = outcome == EmailEnqueueOutcome.Queued;
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
            GeneratedPassword = inviteQueued ? null : generatedPassword,
            InviteEmailQueued = inviteQueued,
            CreatedAt = user.CreatedAt,
            EmailVerificationRequired = emailSettings.CanDeliver && emailSettings.RequireEmailVerification
        };
    }
}
