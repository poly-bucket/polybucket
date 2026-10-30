using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Logout.Repository;

public class LogoutRepository(PolyBucketDbContext context) : ILogoutRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task RevokeRefreshTokenAsync(string token, string reason, string revokedByIp)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReasonRevoked = reason;
            refreshToken.RevokedByIp = revokedByIp;
            await _context.SaveChangesAsync();
        }
    }
}
