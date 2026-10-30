using PolyBucket.Api.Features.Federation.Repository;

namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Repository;

public class CreateFederatedInstanceRepository(IFederationRepository federationRepository) : ICreateFederatedInstanceRepository
{
    public Task<FederatedInstance> AddAsync(FederatedInstance instance, CancellationToken cancellationToken = default) =>
        federationRepository.AddFederatedInstanceAsync(instance);
}
