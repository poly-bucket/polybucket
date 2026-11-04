using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Database.Entities.Users;
using PolyBucket.Email;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Domain;

public class RecoveryCodeResetPasswordCommandHandler(
    IRecoveryCodeResetPasswordRepository repository,
    IRecoveryCodeRepository recoveryCodeRepository,
    IRecoveryCodeService recoveryCodeService,
    IPasswordHasher passwordHasher,
    IAccountEmailService accountEmailService,
    TimeProvider timeProvider,
    ILogger<RecoveryCodeResetPasswordCommandHandler> logger)
{
    public const string InvalidRequestMessage =
        "Invalid recovery code or account identifier. Check your details and try again.";

    public async Task Handle(RecoveryCodeResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByEmailOrUsernameAsync(command.EmailOrUsername, cancellationToken);
        if (user == null)
        {
            logger.LogInformation("Recovery code password reset failed: user not found for identifier");
            throw new InvalidOperationException(InvalidRequestMessage);
        }

        var unusedCodes = await recoveryCodeRepository.GetUnusedCodesForUserAsync(user.Id, cancellationToken);
        if (!recoveryCodeService.TryMatchCode(unusedCodes, command.RecoveryCode, out var matchedCode)
            || matchedCode == null)
        {
            logger.LogInformation("Recovery code password reset failed for user {UserId}", user.Id);
            throw new InvalidOperationException(InvalidRequestMessage);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        matchedCode.IsUsed = true;
        matchedCode.UsedAt = now;
        matchedCode.UpdatedAt = now;
        matchedCode.UpdatedById = user.Id;

        var salt = passwordHasher.GenerateSalt();
        user.PasswordHash = passwordHasher.HashPassword(command.NewPassword, salt);
        user.Salt = salt;
        user.RequiresPasswordChange = false;
        user.EmailVerifiedAt ??= now;
        user.UpdatedAt = now;

        repository.AddAuditLog(new UserAuditLog
        {
            UserId = user.Id,
            PerformedByUserId = user.Id,
            Action = UserAuditAction.RecoveryCodePasswordReset,
            Details = "Password reset using account recovery code",
            IpAddress = command.Client.IpAddress,
            CreatedAt = now
        });

        await repository.SaveChangesAsync(cancellationToken);
        await repository.InvalidateOutstandingPasswordResetTokensAsync(user.Email, now, cancellationToken);
        await repository.RevokeAllRefreshTokensForUserAsync(user.Id, "Password reset via recovery code", command.Client.IpAddress);
        await accountEmailService.SendPasswordChangedAsync(user, command.Client, cancellationToken: cancellationToken);

        logger.LogInformation("Password reset via recovery code for user {UserId}", user.Id);
    }
}
