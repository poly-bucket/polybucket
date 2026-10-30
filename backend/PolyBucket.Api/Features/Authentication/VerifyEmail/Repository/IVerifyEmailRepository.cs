using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Repository;

public interface IVerifyEmailRepository
{
    Task<EmailVerificationToken?> GetEmailVerificationTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<User?> GetUserForUpdateByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserForUpdateByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenAsync(string email);
    Task<bool> TryConsumeEmailVerificationTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
