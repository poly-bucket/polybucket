using PolyBucket.Database.Entities.Models;
using UserEntity = PolyBucket.Database.Entities.Models.User;

namespace PolyBucket.Api.Features.Authentication.DeleteAccount.Repository;

public interface IDeleteAccountRepository
{
    Task<UserEntity?> GetUserForDeleteAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> CountActiveAdminsWithRoleAsync(Guid roleId, CancellationToken cancellationToken);
    Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
