using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Repository;

public interface IResetPasswordRepository
{
    Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<User?> GetUserForUpdateByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> TryConsumePasswordResetTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> InvalidateOutstandingPasswordResetTokensAsync(string email, DateTime usedAt, CancellationToken cancellationToken = default);
    Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp);
}
