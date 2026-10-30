namespace PolyBucket.Api.Features.Federation.GetFederatedInstances.Repository;

public interface IGetFederatedInstancesRepository
{
    Task<IReadOnlyList<FederatedInstance>> GetAllAsync(CancellationToken cancellationToken = default);
}
