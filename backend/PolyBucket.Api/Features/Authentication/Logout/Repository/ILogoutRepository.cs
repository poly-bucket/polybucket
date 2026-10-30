using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Logout.Repository;

public interface ILogoutRepository
{
    Task<RefreshTokenModel?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string reason, string revokedByIp);
}
