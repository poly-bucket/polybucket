namespace PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Repository;

public interface IUpdateFederatedInstanceRepository
{
    Task<FederatedInstance?> GetFederatedInstanceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FederatedInstance> UpdateFederatedInstanceAsync(FederatedInstance instance, CancellationToken cancellationToken = default);
    Task DeleteFederatedInstanceAsync(Guid id, CancellationToken cancellationToken = default);
}
