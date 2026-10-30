using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.RevokeSession.Repository;

public interface IRevokeSessionRepository
{
    Task<RefreshTokenModel?> GetActiveRefreshTokenByIdAsync(Guid tokenId, Guid userId);
    Task RevokeRefreshTokenByIdAsync(Guid tokenId, string reason, string revokedByIp);
}
