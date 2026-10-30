using PolyBucket.Api.Features.Federation.Repository;

namespace PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Repository;

public class UpdateFederatedInstanceRepository(IFederationRepository federationRepository) : IUpdateFederatedInstanceRepository
{
    public Task<FederatedInstance?> GetFederatedInstanceAsync(Guid id, CancellationToken cancellationToken = default) =>
        federationRepository.GetFederatedInstanceAsync(id);

    public Task<FederatedInstance> UpdateFederatedInstanceAsync(FederatedInstance instance, CancellationToken cancellationToken = default) =>
        federationRepository.UpdateFederatedInstanceAsync(instance);

    public Task DeleteFederatedInstanceAsync(Guid id, CancellationToken cancellationToken = default) =>
        federationRepository.DeleteFederatedInstanceAsync(id);
}
