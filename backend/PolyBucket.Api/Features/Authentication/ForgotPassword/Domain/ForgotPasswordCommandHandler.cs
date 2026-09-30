using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Domain
{
    public class ForgotPasswordCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IAccountEmailService accountEmailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        public static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IAccountEmailService _accountEmailService = accountEmailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

        public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _authRepository.GetUserByEmailAsync(command.Email);
            if (user == null)
            {
                _logger.LogInformation("Password reset requested for an unknown email address");
                return;
            }

            var resetToken = _tokenService.GeneratePasswordResetToken();
            var passwordResetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash(resetToken),
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.Add(ResetTokenLifetime),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = command.Client.IpAddress
            };

            await _authRepository.CreatePasswordResetTokenAsync(passwordResetToken);
            var outcome = await _accountEmailService.SendPasswordResetAsync(user, resetToken, ResetTokenLifetime, cancellationToken: cancellationToken);

            _logger.LogInformation("Password reset email for user {UserId}: {Outcome}", user.Id, outcome);
        }
    }
}
