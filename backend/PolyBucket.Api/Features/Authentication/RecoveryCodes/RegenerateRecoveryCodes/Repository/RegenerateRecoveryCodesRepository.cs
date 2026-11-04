using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Repository;

public sealed class RegenerateRecoveryCodesRepository(PolyBucketDbContext context) : IRegenerateRecoveryCodesRepository
{
    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
