using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Domain
{
    public class ResetPasswordCommandHandler(
        IAuthenticationRepository authRepository,
        IPasswordHasher passwordHasher,
        IAccountEmailService accountEmailService,
        TimeProvider timeProvider,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        public const string InvalidTokenMessage = "This password reset link is invalid or has expired. Request a new one.";

        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IAccountEmailService _accountEmailService = accountEmailService;
        private readonly TimeProvider _timeProvider = timeProvider;
        private readonly ILogger<ResetPasswordCommandHandler> _logger = logger;

        public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Token))
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var resetToken = await _authRepository.GetPasswordResetTokenByHashAsync(TokenHasher.Hash(command.Token.Trim()), cancellationToken);
            if (resetToken == null
                || resetToken.IsUsed
                || resetToken.ExpiresAt <= now
                || (!string.IsNullOrWhiteSpace(command.Email) && !string.Equals(resetToken.Email, command.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            var user = await _authRepository.GetUserForUpdateByEmailAsync(resetToken.Email, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            if (!await _authRepository.TryConsumePasswordResetTokenAsync(resetToken.Id, now, cancellationToken))
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            var salt = _passwordHasher.GenerateSalt();
            user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword, salt);
            user.Salt = salt;
            user.RequiresPasswordChange = false;
            user.EmailVerifiedAt ??= now;
            user.UpdatedAt = now;
            await _authRepository.SaveChangesAsync(cancellationToken);

            await _authRepository.InvalidateOutstandingPasswordResetTokensAsync(user.Email, now, cancellationToken);
            await _authRepository.RevokeAllRefreshTokensForUserAsync(user.Id, "Password reset", command.Client.IpAddress);
            await _accountEmailService.SendPasswordChangedAsync(user, command.Client, cancellationToken: cancellationToken);

            _logger.LogInformation("Password reset for user {UserId}; all sessions revoked", user.Id);
        }
    }
}
