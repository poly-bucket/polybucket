using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Repository;

public class ChangePasswordRepository(PolyBucketDbContext context) : IChangePasswordRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<User?> GetUserForPasswordChangeAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshTokenModel>> GetActiveRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAdminConfiguredAsync(Guid userId, CancellationToken cancellationToken)
    {
        var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup != null)
        {
            systemSetup.IsAdminConfigured = true;
            systemSetup.UpdatedAt = DateTime.UtcNow;
            systemSetup.UpdatedById = userId;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
