using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.DislikeModel.Repository;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models.Enums;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.DislikeModel.Domain;

public interface IDislikeModelService
{
    Task<ModelReactionOutcome> DislikeAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}

public class DislikeModelService(
    IDislikeModelRepository repository,
    IPermissionService permissionService,
    ILogger<DislikeModelService> logger) : IDislikeModelService
{
    public async Task<ModelReactionOutcome> DislikeAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (!TryResolveUserId(user, out var userId))
        {
            return ModelReactionOutcome.Unauthorized;
        }

        if (!await repository.IsReactionsEnabledAsync(cancellationToken))
        {
            return ModelReactionOutcome.Disabled;
        }

        var model = await repository.GetModelByIdAsync(modelId, cancellationToken);
        if (model == null)
        {
            return ModelReactionOutcome.NotFound;
        }

        if (model.AuthorId == userId)
        {
            return ModelReactionOutcome.Forbidden;
        }

        if (!await CanUserAccessModelAsync(model, userId, cancellationToken))
        {
            return ModelReactionOutcome.Unauthorized;
        }

        var existing = await repository.GetUserReactionAsync(modelId, userId, cancellationToken);
        var applied = existing switch
        {
            null => await repository.TryAddAsync(modelId, userId, ModelReactionType.Dislike, cancellationToken),
            var current when current == ModelReactionType.Dislike => false,
            var current => await repository.TrySwitchAsync(modelId, userId, current.Value, ModelReactionType.Dislike, cancellationToken)
        };

        if (applied)
        {
            logger.LogInformation("User {UserId} disliked model {ModelId}", userId, modelId);
        }

        return await BuildOutcomeAsync(modelId, userId, applied, cancellationToken);
    }

    private async Task<ModelReactionOutcome> BuildOutcomeAsync(Guid modelId, Guid userId, bool applied, CancellationToken cancellationToken)
    {
        var (likes, dislikes) = await repository.GetCountsAsync(modelId, cancellationToken);
        var current = await repository.GetUserReactionAsync(modelId, userId, cancellationToken);
        return new ModelReactionOutcome(
            applied ? ModelReactionChange.Applied : ModelReactionChange.Unchanged,
            likes,
            dislikes,
            current);
    }

    private static bool TryResolveUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
        {
            userId = default;
            return false;
        }

        return true;
    }

    private async Task<bool> CanUserAccessModelAsync(Model model, Guid currentUserId, CancellationToken cancellationToken)
    {
        if (model.Privacy == PrivacySettings.Public)
        {
            return true;
        }

        if (model.AuthorId == currentUserId)
        {
            return true;
        }

        var isAdmin = await permissionService.IsAdminAsync(currentUserId);
        var userRole = await permissionService.GetUserRoleAsync(currentUserId);
        var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
        if (isAdmin || isModerator)
        {
            return true;
        }

        if (model.Privacy == PrivacySettings.Private)
        {
            return false;
        }

        if (model.Privacy == PrivacySettings.Unlisted)
        {
            return true;
        }

        return false;
    }
}
