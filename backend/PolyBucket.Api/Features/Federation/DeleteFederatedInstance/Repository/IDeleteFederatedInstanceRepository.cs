namespace PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Repository;

public interface IDeleteFederatedInstanceRepository
{
    Task<FederatedInstance?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
