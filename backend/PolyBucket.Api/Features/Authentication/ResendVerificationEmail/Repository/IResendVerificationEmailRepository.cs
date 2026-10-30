using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Repository;

public interface IResendVerificationEmailRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<DateTime?> GetLatestEmailVerificationTokenCreatedAtAsync(string email, EmailVerificationPurpose purpose, CancellationToken cancellationToken = default);
    Task<int> InvalidateOutstandingEmailVerificationTokensAsync(string email, EmailVerificationPurpose purpose, DateTime usedAt, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token);
}
