using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Repository;

public sealed class GetPendingRecoveryCodesDisplayRepository(PolyBucketDbContext context) : IGetPendingRecoveryCodesDisplayRepository
{
    public Task<User?> GetUserWithRoleAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
}
