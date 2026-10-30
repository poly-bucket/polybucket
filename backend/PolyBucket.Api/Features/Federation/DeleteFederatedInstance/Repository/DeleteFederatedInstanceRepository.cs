using PolyBucket.Api.Features.Federation.Repository;

namespace PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Repository;

public class DeleteFederatedInstanceRepository(IFederationRepository federationRepository) : IDeleteFederatedInstanceRepository
{
    public Task<FederatedInstance?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        federationRepository.GetFederatedInstanceAsync(id);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        federationRepository.DeleteFederatedInstanceAsync(id);
}
