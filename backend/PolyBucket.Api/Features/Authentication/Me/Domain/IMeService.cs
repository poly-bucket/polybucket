namespace PolyBucket.Api.Features.Authentication.Me.Domain;

public interface IMeService
{
    Task<MeResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
