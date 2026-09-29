using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.AddTagToModel.Domain;

public interface IAddTagToModelService
{
    Task AddTagToModelAsync(Guid modelId, string? tagName, ClaimsPrincipal user, CancellationToken cancellationToken);
}
