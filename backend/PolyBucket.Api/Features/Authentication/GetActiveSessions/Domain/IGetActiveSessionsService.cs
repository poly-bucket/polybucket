namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Domain;

public interface IGetActiveSessionsService
{
    Task<GetActiveSessionsResponse> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken);
}
