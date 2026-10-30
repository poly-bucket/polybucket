namespace PolyBucket.Api.Features.Federation.GetFederationHealth.Repository;

public interface IGetFederationHealthRepository
{
    Task<FederatedInstance?> GetFederatedInstanceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FederatedInstance> UpdateFederatedInstanceAsync(FederatedInstance instance, CancellationToken cancellationToken = default);
    Task DeleteFederatedInstanceAsync(Guid id, CancellationToken cancellationToken = default);
}
