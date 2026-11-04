using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Repository;

public interface IRecoveryCodeResetPasswordRepository
{
    Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<int> InvalidateOutstandingPasswordResetTokensAsync(string email, DateTime usedAt, CancellationToken cancellationToken = default);

    Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp);

    void AddAuditLog(UserAuditLog auditLog);
}
