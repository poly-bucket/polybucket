using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.RemoveTagFromModel.Domain;

public interface IRemoveTagFromModelService
{
    Task RemoveTagFromModelAsync(Guid modelId, Guid tagId, ClaimsPrincipal user, CancellationToken cancellationToken);
}
