using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Repository;

public class DeleteAllModelsUserRepository(PolyBucketDbContext context) : IDeleteAllModelsUserRepository
{
    public Task<User?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(u => u.Settings)
            .Include(u => u.Logins)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
