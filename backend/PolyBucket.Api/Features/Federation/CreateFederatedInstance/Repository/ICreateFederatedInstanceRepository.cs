namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Repository;

public interface ICreateFederatedInstanceRepository
{
    Task<FederatedInstance> AddAsync(FederatedInstance instance, CancellationToken cancellationToken = default);
}
