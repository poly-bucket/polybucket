using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.SkipPasswordChange.Repository;

public class SkipPasswordChangeRepository(PolyBucketDbContext context) : ISkipPasswordChangeRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<User?> GetUserForSkipAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
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
