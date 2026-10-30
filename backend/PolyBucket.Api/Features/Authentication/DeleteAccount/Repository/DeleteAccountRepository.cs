using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using UserEntity = PolyBucket.Database.Entities.Models.User;

namespace PolyBucket.Api.Features.Authentication.DeleteAccount.Repository;

public class DeleteAccountRepository(PolyBucketDbContext context) : IDeleteAccountRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<UserEntity?> GetUserForDeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Role)
            .Include(u => u.Settings)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<int> CountActiveAdminsWithRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return _context.Users.CountAsync(u => u.RoleId == roleId && u.CanLogin, cancellationToken);
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

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
