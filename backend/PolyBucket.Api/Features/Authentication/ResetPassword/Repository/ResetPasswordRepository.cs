using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Repository;

public class ResetPasswordRepository(PolyBucketDbContext context) : IResetPasswordRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _context.PasswordResetTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
    }

    public Task<User?> GetUserForUpdateByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> TryConsumePasswordResetTokenAsync(Guid tokenId, DateTime usedAt, CancellationToken cancellationToken = default)
    {
        var updated = await _context.PasswordResetTokens
            .Where(t => t.Id == tokenId && !t.IsUsed && t.ExpiresAt > usedAt)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsUsed, true)
                .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
        return updated == 1;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> InvalidateOutstandingPasswordResetTokensAsync(string email, DateTime usedAt, CancellationToken cancellationToken = default)
    {
        return _context.PasswordResetTokens
            .Where(t => t.Email == email && !t.IsUsed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsUsed, true)
                .SetProperty(t => t.UsedAt, usedAt), cancellationToken);
    }

    public async Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp)
    {
        var userRefreshTokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToListAsync();
        foreach (var token in userRefreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = reason;
            token.RevokedByIp = revokedByIp;
        }

        await _context.SaveChangesAsync();
    }
}
