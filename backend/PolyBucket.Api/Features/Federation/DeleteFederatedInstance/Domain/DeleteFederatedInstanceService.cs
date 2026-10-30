using PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Repository;

namespace PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Domain;

public class DeleteFederatedInstanceService(IDeleteFederatedInstanceRepository repository) : IDeleteFederatedInstanceService
{
    public async Task<DeleteFederatedInstanceOutcome> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await repository.GetAsync(id, cancellationToken) == null)
            return DeleteFederatedInstanceOutcome.NotFound;
        await repository.DeleteAsync(id, cancellationToken);
        return DeleteFederatedInstanceOutcome.Deleted;
    }
}
