using PolyBucket.Api.Features.Federation.Repository;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstances.Repository;

public class GetFederatedInstancesRepository(IFederationRepository federationRepository) : IGetFederatedInstancesRepository
{
    public async Task<IReadOnlyList<FederatedInstance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await federationRepository.GetFederatedInstancesAsync();
        return items.ToList();
    }
}
