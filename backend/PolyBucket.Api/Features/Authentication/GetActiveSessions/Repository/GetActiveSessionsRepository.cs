using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Repository;

public class GetActiveSessionsRepository(PolyBucketDbContext context) : IGetActiveSessionsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<IReadOnlyList<RefreshTokenModel>> GetActiveRefreshTokensForUserAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }
}
