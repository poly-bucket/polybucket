using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Repository;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Domain
{
    public sealed record VerifyEmailResult(EmailVerificationPurpose Purpose, string Email);

    public class VerifyEmailCommandHandler(
        IVerifyEmailRepository verifyEmailRepository,
        IAccountEmailService accountEmailService,
        TimeProvider timeProvider,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        public const string InvalidTokenMessage = "This verification link is invalid or has expired. Request a new one.";
        public const string EmailInUseMessage = "This email address is already in use by another account.";

        private readonly IVerifyEmailRepository _verifyEmailRepository = verifyEmailRepository;
        private readonly IAccountEmailService _accountEmailService = accountEmailService;
        private readonly TimeProvider _timeProvider = timeProvider;
        private readonly ILogger<VerifyEmailCommandHandler> _logger = logger;

        public async Task<VerifyEmailResult> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Token))
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var token = await _verifyEmailRepository.GetEmailVerificationTokenByHashAsync(TokenHasher.Hash(command.Token.Trim()), cancellationToken);
            if (token == null
                || token.IsUsed
                || token.ExpiresAt <= now
                || (!string.IsNullOrWhiteSpace(command.Email) && !string.Equals(token.Email, command.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            var user = token.Purpose == EmailVerificationPurpose.ChangeAddress
                ? token.UserId.HasValue ? await _verifyEmailRepository.GetUserForUpdateByIdAsync(token.UserId.Value, cancellationToken) : null
                : await _verifyEmailRepository.GetUserForUpdateByEmailAsync(token.Email, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            if (token.Purpose == EmailVerificationPurpose.ChangeAddress)
            {
                if (!string.Equals(user.PendingEmail, token.Email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(InvalidTokenMessage);
                }

                if (await _verifyEmailRepository.IsEmailTakenAsync(token.Email))
                {
                    throw new InvalidOperationException(EmailInUseMessage);
                }
            }

            if (!await _verifyEmailRepository.TryConsumeEmailVerificationTokenAsync(token.Id, now, cancellationToken))
            {
                throw new InvalidOperationException(InvalidTokenMessage);
            }

            return token.Purpose == EmailVerificationPurpose.ChangeAddress
                ? await CompleteEmailChangeAsync(user, token.Email, now, cancellationToken)
                : await CompleteVerificationAsync(user, now, cancellationToken);
        }

        private async Task<VerifyEmailResult> CompleteVerificationAsync(User user, DateTime now, CancellationToken cancellationToken)
        {
            user.EmailVerifiedAt ??= now;
            user.UpdatedAt = now;
            await _verifyEmailRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified for user {UserId}", user.Id);
            return new VerifyEmailResult(EmailVerificationPurpose.VerifyAddress, user.Email);
        }

        private async Task<VerifyEmailResult> CompleteEmailChangeAsync(User user, string newEmail, DateTime now, CancellationToken cancellationToken)
        {
            var previousEmail = user.Email;
            user.Email = newEmail;
            user.PendingEmail = null;
            user.EmailVerifiedAt = now;
            user.UpdatedAt = now;
            await _verifyEmailRepository.SaveChangesAsync(cancellationToken);

            await _accountEmailService.SendEmailChangedNoticeAsync(user, previousEmail, newEmail, cancellationToken: cancellationToken);

            _logger.LogInformation("Email address changed for user {UserId}", user.Id);
            return new VerifyEmailResult(EmailVerificationPurpose.ChangeAddress, newEmail);
        }
    }
}
