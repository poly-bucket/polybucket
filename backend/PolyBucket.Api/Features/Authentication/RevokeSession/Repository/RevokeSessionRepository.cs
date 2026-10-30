using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.RevokeSession.Repository;

public class RevokeSessionRepository(PolyBucketDbContext context) : IRevokeSessionRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<RefreshTokenModel?> GetActiveRefreshTokenByIdAsync(Guid tokenId, Guid userId)
    {
        return _context.RefreshTokens
            .Where(rt => rt.Id == tokenId && rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task RevokeRefreshTokenByIdAsync(Guid tokenId, string reason, string revokedByIp)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == tokenId);
        if (refreshToken == null || refreshToken.RevokedAt != null)
        {
            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = reason;
        refreshToken.RevokedByIp = revokedByIp;
        await _context.SaveChangesAsync();
    }
}
