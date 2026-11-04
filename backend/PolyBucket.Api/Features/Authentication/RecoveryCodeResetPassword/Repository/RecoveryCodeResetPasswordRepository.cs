using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Repository;

public sealed class RecoveryCodeResetPasswordRepository(PolyBucketDbContext context) : IRecoveryCodeResetPasswordRepository
{
    public async Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        var normalized = emailOrUsername.Trim();
        if (normalized.Contains('@', StringComparison.Ordinal))
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized.ToLower(), cancellationToken);
        }

        return await context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task<int> InvalidateOutstandingPasswordResetTokensAsync(string email, DateTime usedAt, CancellationToken cancellationToken = default)
    {
        return await context.PasswordResetTokens
            .Where(t => t.Email == email && !t.IsUsed)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.IsUsed, true)
                    .SetProperty(t => t.UsedAt, usedAt),
                cancellationToken);
    }

    public async Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp)
    {
        var now = DateTime.UtcNow;
        await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.ReasonRevoked, reason)
                .SetProperty(t => t.RevokedByIp, revokedByIp));
    }

    public void AddAuditLog(UserAuditLog auditLog) => context.UserAuditLogs.Add(auditLog);
}
