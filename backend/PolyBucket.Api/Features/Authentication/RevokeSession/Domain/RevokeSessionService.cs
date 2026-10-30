using PolyBucket.Api.Features.Authentication.RevokeSession.Repository;

namespace PolyBucket.Api.Features.Authentication.RevokeSession.Domain;

public class RevokeSessionService(IRevokeSessionRepository repository) : IRevokeSessionService
{
    private readonly IRevokeSessionRepository _repository = repository;

    public async Task<RevokeSessionResult> RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _repository.GetActiveRefreshTokenByIdAsync(sessionId, userId);
        if (session == null)
        {
            return RevokeSessionResult.NotFound();
        }

        await _repository.RevokeRefreshTokenByIdAsync(sessionId, "User revoked one session", "127.0.0.1");
        return RevokeSessionResult.Success();
    }
}
