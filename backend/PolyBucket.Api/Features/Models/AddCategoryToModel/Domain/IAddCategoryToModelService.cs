using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;

public interface IAddCategoryToModelService
{
    Task AddCategoryToModelAsync(Guid modelId, Guid categoryId, ClaimsPrincipal user, CancellationToken cancellationToken);
}
