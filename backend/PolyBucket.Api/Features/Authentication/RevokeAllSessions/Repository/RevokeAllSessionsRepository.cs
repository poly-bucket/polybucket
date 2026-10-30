using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.RevokeAllSessions.Repository;

public class RevokeAllSessionsRepository(PolyBucketDbContext context) : IRevokeAllSessionsRepository
{
    private readonly PolyBucketDbContext _context = context;

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
