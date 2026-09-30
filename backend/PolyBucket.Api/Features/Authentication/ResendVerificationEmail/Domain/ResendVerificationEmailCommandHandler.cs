using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Register.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Domain
{
    public enum ResendVerificationOutcome
    {
        Sent,
        EmailUnavailable,
        UnknownOrVerified,
        Throttled
    }

    public class ResendVerificationEmailCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IEmailSettingsResolver emailSettingsResolver,
        IAccountEmailService accountEmailService,
        TimeProvider timeProvider,
        ILogger<ResendVerificationEmailCommandHandler> logger)
    {
        public static readonly TimeSpan ResendThrottle = TimeSpan.FromSeconds(60);

        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;
        private readonly IAccountEmailService _accountEmailService = accountEmailService;
        private readonly TimeProvider _timeProvider = timeProvider;
        private readonly ILogger<ResendVerificationEmailCommandHandler> _logger = logger;

        public async Task<ResendVerificationOutcome> Handle(ResendVerificationEmailCommand command, CancellationToken cancellationToken)
        {
            var settings = await _emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
            if (!settings.CanDeliver)
            {
                _logger.LogWarning("Verification resend requested but email delivery is not configured");
                return ResendVerificationOutcome.EmailUnavailable;
            }

            var user = await _authRepository.GetUserByEmailAsync(command.Email.Trim());
            if (user == null || user.EmailVerifiedAt.HasValue)
            {
                return ResendVerificationOutcome.UnknownOrVerified;
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var latest = await _authRepository.GetLatestEmailVerificationTokenCreatedAtAsync(user.Email, EmailVerificationPurpose.VerifyAddress, cancellationToken);
            if (latest.HasValue && now - latest.Value < ResendThrottle)
            {
                _logger.LogInformation("Verification resend throttled for user {UserId}", user.Id);
                return ResendVerificationOutcome.Throttled;
            }

            await _authRepository.InvalidateOutstandingEmailVerificationTokensAsync(user.Email, EmailVerificationPurpose.VerifyAddress, now, cancellationToken);

            var rawToken = _tokenService.GenerateEmailVerificationToken();
            await _authRepository.CreateEmailVerificationTokenAsync(new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash(rawToken),
                Email = user.Email,
                UserId = user.Id,
                Purpose = EmailVerificationPurpose.VerifyAddress,
                ExpiresAt = now.Add(RegisterCommandHandler.EmailVerificationLifetime),
                CreatedAt = now,
                CreatedByIp = command.Client.IpAddress
            });

            var outcome = await _accountEmailService.SendVerificationAsync(user, rawToken, RegisterCommandHandler.EmailVerificationLifetime, cancellationToken: cancellationToken);
            _logger.LogInformation("Verification email resent for user {UserId}: {Outcome}", user.Id, outcome);
            return ResendVerificationOutcome.Sent;
        }
    }
}
