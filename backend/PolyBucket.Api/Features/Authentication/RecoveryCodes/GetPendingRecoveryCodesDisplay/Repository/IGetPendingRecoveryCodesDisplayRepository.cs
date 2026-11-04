using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Repository;

public interface IGetPendingRecoveryCodesDisplayRepository
{
    Task<User?> GetUserWithRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}
