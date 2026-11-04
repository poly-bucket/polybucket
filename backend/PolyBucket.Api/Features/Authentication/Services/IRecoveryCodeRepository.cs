using PolyBucket.Database.Entities.Authentication;

namespace PolyBucket.Api.Features.Authentication.Services;

public interface IRecoveryCodeRepository
{
    Task AddCodesAsync(IEnumerable<AccountRecoveryCode> codes, CancellationToken cancellationToken = default);

    Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountRecoveryCode>> GetUnusedCodesForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
