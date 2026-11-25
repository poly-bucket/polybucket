using PolyBucket.Api.Features.Authentication.GetActiveSessions.Repository;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Domain;

public class GetActiveSessionsService(
    IGetActiveSessionsRepository repository,
    ITokenSettingsService tokenSettingsService) : IGetActiveSessionsService
{
    private readonly IGetActiveSessionsRepository _repository = repository;
    private readonly ITokenSettingsService _tokenSettingsService = tokenSettingsService;

    public async Task<GetActiveSessionsResponse> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!await _tokenSettingsService.IsRefreshTokensEnabledAsync())
        {
            return new GetActiveSessionsResponse { Sessions = [] };
        }

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
