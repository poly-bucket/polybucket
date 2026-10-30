using RefreshTokenModel = PolyBucket.Database.Entities.Authentication.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Repository;

public interface IGetActiveSessionsRepository
{
    Task<IReadOnlyList<RefreshTokenModel>> GetActiveRefreshTokensForUserAsync(Guid userId);
}
