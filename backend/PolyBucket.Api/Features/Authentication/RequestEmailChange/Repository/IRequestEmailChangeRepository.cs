using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.RequestEmailChange.Repository;

public interface IRequestEmailChangeRepository
{
    Task<User?> GetUserForUpdateByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenAsync(string email);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> InvalidateOutstandingEmailVerificationTokensAsync(string email, EmailVerificationPurpose purpose, DateTime usedAt, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken token);
}
