using PolyBucket.Api.Features.Authentication.RevokeAllSessions.Repository;

namespace PolyBucket.Api.Features.Authentication.RevokeAllSessions.Domain;

public interface IRevokeAllSessionsService
{
    Task RevokeAllSessionsAsync(Guid userId, CancellationToken cancellationToken);
}

public class RevokeAllSessionsService(IRevokeAllSessionsRepository repository) : IRevokeAllSessionsService
{
    private readonly IRevokeAllSessionsRepository _repository = repository;

    public Task RevokeAllSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _repository.RevokeAllRefreshTokensForUserAsync(
            userId,
            "User revoked all sessions",
            "127.0.0.1");
    }
}
