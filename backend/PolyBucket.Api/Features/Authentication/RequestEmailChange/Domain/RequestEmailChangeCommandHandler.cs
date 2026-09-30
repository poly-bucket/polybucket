using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.RequestEmailChange.Domain
{
    public class RequestEmailChangeCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IEmailSettingsResolver emailSettingsResolver,
        IAccountEmailService accountEmailService,
        TimeProvider timeProvider,
        ILogger<RequestEmailChangeCommandHandler> logger)
    {
        public static readonly TimeSpan EmailChangeLifetime = TimeSpan.FromHours(24);
        public const string EmailUnavailableMessage = "Email delivery is not configured on this server. Ask an administrator to change your email address.";
        public const string IncorrectPasswordMessage = "Current password is incorrect";
        public const string SameEmailMessage = "The new email address matches your current one.";
        public const string EmailInUseMessage = "This email address is already in use by another account.";

        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;
        private readonly IAccountEmailService _accountEmailService = accountEmailService;
        private readonly TimeProvider _timeProvider = timeProvider;
        private readonly ILogger<RequestEmailChangeCommandHandler> _logger = logger;

        public async Task Handle(RequestEmailChangeCommand command, CancellationToken cancellationToken)
        {
            var settings = await _emailSettingsResolver.GetEffectiveSettingsAsync(cancellationToken);
            if (!settings.CanDeliver || !settings.HasPublicBaseUrl)
            {
                throw new InvalidOperationException(EmailUnavailableMessage);
            }

            var user = await _authRepository.GetUserForUpdateByIdAsync(command.UserId, cancellationToken)
                ?? throw new UnauthorizedAccessException("User not found");

            if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException(IncorrectPasswordMessage);
            }

            var newEmail = command.NewEmail.Trim();
            if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(SameEmailMessage);
            }

            if (await _authRepository.IsEmailTakenAsync(newEmail))
            {
                throw new InvalidOperationException(EmailInUseMessage);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            user.PendingEmail = newEmail;
            user.UpdatedAt = now;
            await _authRepository.SaveChangesAsync(cancellationToken);

            await _authRepository.InvalidateOutstandingEmailVerificationTokensAsync(newEmail, EmailVerificationPurpose.ChangeAddress, now, cancellationToken);

            var rawToken = _tokenService.GenerateEmailVerificationToken();
            await _authRepository.CreateEmailVerificationTokenAsync(new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash(rawToken),
                Email = newEmail,
                UserId = user.Id,
                Purpose = EmailVerificationPurpose.ChangeAddress,
                ExpiresAt = now.Add(EmailChangeLifetime),
                CreatedAt = now,
                CreatedByIp = command.Client.IpAddress
            });

            var outcome = await _accountEmailService.SendEmailChangeRequestedAsync(user, newEmail, rawToken, EmailChangeLifetime, cancellationToken: cancellationToken);
            _logger.LogInformation("Email change requested for user {UserId}: {Outcome}", user.Id, outcome);
        }
    }
}
