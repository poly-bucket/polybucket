namespace PolyBucket.Api.Features.Federation.GetFederatedInstance.Repository;

public interface IGetFederatedInstanceRepository
{
    Task<FederatedInstance?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
