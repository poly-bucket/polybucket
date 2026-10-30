using PolyBucket.Api.Features.Authentication.GetActiveSessions.Repository;

namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Domain;

public class GetActiveSessionsService(IGetActiveSessionsRepository repository) : IGetActiveSessionsService
{
    private readonly IGetActiveSessionsRepository _repository = repository;

    public async Task<GetActiveSessionsResponse> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetActiveRefreshTokensForUserAsync(userId);
        return new GetActiveSessionsResponse
        {
            Sessions = sessions.Select(s => new SessionSummaryDto
            {
                SessionId = s.Id,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                CreatedByIp = s.CreatedByIp,
            }).ToList(),
        };
    }
}
