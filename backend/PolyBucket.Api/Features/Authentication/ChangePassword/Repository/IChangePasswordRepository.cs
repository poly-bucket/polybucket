using PolyBucket.Database.Entities.Models;
using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Repository;

public interface IChangePasswordRepository
{
    Task<User?> GetUserForPasswordChangeAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RefreshTokenModel>> GetActiveRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);
    Task MarkAdminConfiguredAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
