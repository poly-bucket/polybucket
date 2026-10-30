namespace PolyBucket.Api.Features.Authentication.RevokeAllSessions.Repository;

public interface IRevokeAllSessionsRepository
{
    Task RevokeAllRefreshTokensForUserAsync(Guid userId, string reason, string revokedByIp);
}
