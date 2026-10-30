using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.UnlikeModel.Repository;
using PolyBucket.Database.Entities.Models.Enums;
using System.Security.Claims;
using PolyBucket.Api.Common;

namespace PolyBucket.Api.Features.Models.UnlikeModel.Domain;

public interface IUnlikeModelService
{
    Task<ModelReactionOutcome> UnlikeAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}

public class UnlikeModelService(IUnlikeModelRepository repository) : IUnlikeModelService
{
    public async Task<ModelReactionOutcome> UnlikeAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (!TryResolveUserId(user, out var userId)) return ModelReactionOutcome.Unauthorized;
        var model = await repository.GetModelByIdAsync(modelId, cancellationToken);
        if (model == null) return ModelReactionOutcome.NotFound;
        if (model.AuthorId == userId) return ModelReactionOutcome.Forbidden;
        var existing = await repository.GetUserReactionAsync(modelId, userId, cancellationToken);
        var applied = existing == ModelReactionType.Like && await repository.TryRemoveAsync(modelId, userId, ModelReactionType.Like, cancellationToken);
        var (likes, dislikes) = await repository.GetCountsAsync(modelId, cancellationToken);
        var current = await repository.GetUserReactionAsync(modelId, userId, cancellationToken);
        return new ModelReactionOutcome(applied ? ModelReactionChange.Applied : ModelReactionChange.Unchanged, likes, dislikes, current);
    }

    private static bool TryResolveUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId)) { userId = default; return false; }
        return true;
    }
}
