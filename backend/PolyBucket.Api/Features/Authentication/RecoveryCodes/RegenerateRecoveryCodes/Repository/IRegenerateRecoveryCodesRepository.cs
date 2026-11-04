using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Repository;

public interface IRegenerateRecoveryCodesRepository
{
    Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
