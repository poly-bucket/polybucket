using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Domain
{
    public class ResetPasswordCommandHandler
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(
            IAuthenticationRepository authRepository,
            ILogger<ResetPasswordCommandHandler> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            // Get and validate reset token
            var resetToken = await _authRepository.GetPasswordResetTokenAsync(command.Token);
            if (resetToken == null || !resetToken.IsValid || resetToken.Email != command.Email)
            {
                throw new InvalidOperationException("Invalid or expired reset token");
            }

            // Get user
            var user = await _authRepository.GetUserByEmailAsync(command.Email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Hash new password
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword, salt);

            // Update user password
            user.PasswordHash = passwordHash;
            user.Salt = salt;
            user.UpdatedAt = DateTime.UtcNow;

            // Mark token as used
            await _authRepository.MarkPasswordResetTokenAsUsedAsync(command.Token);

            // Revoke all refresh tokens for security
            await _authRepository.RevokeAllRefreshTokensForUserAsync(user.Id, "Password changed", "127.0.0.1");

            _logger.LogInformation("Password reset successfully for user: {Email}", user.Email);
        }
    }
} 