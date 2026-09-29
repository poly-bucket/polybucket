using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Domain;

public interface IRemoveCategoryFromModelService
{
    Task RemoveCategoryFromModelAsync(Guid modelId, Guid categoryId, ClaimsPrincipal user, CancellationToken cancellationToken);
}
