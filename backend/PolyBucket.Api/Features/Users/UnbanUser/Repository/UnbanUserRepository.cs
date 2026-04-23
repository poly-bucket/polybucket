using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Users.UnbanUser.Repository;

public class UnbanUserRepository(PolyBucketDbContext context) : IUnbanUserRepository
{
    public Task<User?> FindByIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
