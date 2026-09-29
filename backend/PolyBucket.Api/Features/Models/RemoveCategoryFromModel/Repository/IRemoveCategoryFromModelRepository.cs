using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Repository;

public interface IRemoveCategoryFromModelRepository
{
    Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
