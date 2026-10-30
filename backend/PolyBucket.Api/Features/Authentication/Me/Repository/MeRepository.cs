using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.Me.Repository;

public class MeRepository(PolyBucketDbContext context) : IMeRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<User?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Role)
            .Include(u => u.Settings)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
}
