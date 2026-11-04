using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.Services;

public sealed class RecoveryCodeRepository(PolyBucketDbContext context) : IRecoveryCodeRepository
{
    public async Task AddCodesAsync(IEnumerable<AccountRecoveryCode> codes, CancellationToken cancellationToken = default)
    {
        await context.AccountRecoveryCodes.AddRangeAsync(codes, cancellationToken);
    }

    public async Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await context.AccountRecoveryCodes
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccountRecoveryCode>> GetUnusedCodesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.AccountRecoveryCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
