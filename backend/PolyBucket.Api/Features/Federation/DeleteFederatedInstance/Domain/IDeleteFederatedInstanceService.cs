namespace PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Domain;

public enum DeleteFederatedInstanceOutcome { Deleted, NotFound }

public interface IDeleteFederatedInstanceService
{
    Task<DeleteFederatedInstanceOutcome> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
