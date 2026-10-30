using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Repository;

public class RefreshTokenRepository(PolyBucketDbContext context) : IRefreshTokenRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
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

    public async Task<RefreshTokenModel> CreateRefreshTokenAsync(RefreshTokenModel refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }
}
