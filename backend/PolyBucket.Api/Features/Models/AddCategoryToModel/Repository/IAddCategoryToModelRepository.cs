using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;

namespace PolyBucket.Api.Features.Models.AddCategoryToModel.Repository;

public interface IAddCategoryToModelRepository
{
    Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken);
    Task<Category?> GetActiveCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
