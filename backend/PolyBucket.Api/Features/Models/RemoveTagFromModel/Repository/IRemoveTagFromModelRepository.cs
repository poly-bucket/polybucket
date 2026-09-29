using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.RemoveTagFromModel.Repository;

public interface IRemoveTagFromModelRepository
{
    Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
