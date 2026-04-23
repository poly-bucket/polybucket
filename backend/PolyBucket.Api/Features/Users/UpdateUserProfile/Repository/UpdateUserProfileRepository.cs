using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Repository;

public class UpdateUserProfileRepository(PolyBucketDbContext dbContext) : IUpdateUserProfileRepository
{
    public Task<User?> GetUserByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
