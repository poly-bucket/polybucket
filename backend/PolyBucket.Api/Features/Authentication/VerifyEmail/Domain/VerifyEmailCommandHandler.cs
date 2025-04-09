using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Domain
{
    public class VerifyEmailCommandHandler
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;

        public VerifyEmailCommandHandler(
            IAuthenticationRepository authRepository,
            ILogger<VerifyEmailCommandHandler> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            // Get and validate verification token
            var verificationToken = await _authRepository.GetEmailVerificationTokenAsync(command.Token);
            if (verificationToken == null || !verificationToken.IsValid || verificationToken.Email != command.Email)
            {
                throw new InvalidOperationException("Invalid or expired verification token");
            }

            // Get user
            var user = await _authRepository.GetUserByEmailAsync(command.Email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Mark token as used
            await _authRepository.MarkEmailVerificationTokenAsUsedAsync(command.Token);

            // Update user (in a real implementation, you might have an EmailVerified field)
            user.UpdatedAt = DateTime.UtcNow;
            // TODO: Add EmailVerified field to User model and set it to true

            _logger.LogInformation("Email verified successfully for user: {Email}", user.Email);
        }
    }
} 