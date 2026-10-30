using PolyBucket.Api.Features.Federation.Repository;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstance.Repository;

public class GetFederatedInstanceRepository(IFederationRepository federationRepository) : IGetFederatedInstanceRepository
{
    public Task<FederatedInstance?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        federationRepository.GetFederatedInstanceAsync(id);
}
