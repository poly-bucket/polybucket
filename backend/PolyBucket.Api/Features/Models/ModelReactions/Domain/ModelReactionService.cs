using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.ModelReactions.Repository;
using PolyBucket.Api.Features.Notifications.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.ModelReactions.Domain;

public class ModelReactionService(
    IModelReactionRepository repository,
    IPermissionService permissionService,
    INotificationPublisher notificationPublisher,
    ILogger<ModelReactionService> logger) : IModelReactionService
{
    public async Task<ModelReactionOutcome> ReactAsync(Guid modelId, ClaimsPrincipal user, ModelReactionType type, CancellationToken cancellationToken = default)
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
            null => await repository.TryAddAsync(modelId, userId, type, cancellationToken),
            var current when current == type => false,
            var current => await repository.TrySwitchAsync(modelId, userId, current.Value, type, cancellationToken)
        };

        if (applied && type == ModelReactionType.Like && existing != ModelReactionType.Like)
        {
            await notificationPublisher.PublishAsync(new NotificationRequest
            {
                RecipientUserId = model.AuthorId,
                ActorUserId = userId,
                Type = NotificationType.ModelLiked,
                Title = $"{NotificationRequest.ActorToken} liked your model",
                Message = $"{NotificationRequest.ActorToken} liked \"{model.Name}\".",
                ActionUrl = $"/models/{model.Id}",
                RelatedEntityId = model.Id,
                RelatedEntityType = "Model",
                Priority = NotificationPriority.Low,
                DedupeKey = $"model-like:{model.Id}:{userId}",
                SendEmail = false
            }, cancellationToken);
        }

        if (applied)
        {
            logger.LogInformation("User {UserId} reacted {Reaction} on model {ModelId}", userId, type, modelId);
        }

        return await BuildOutcomeAsync(modelId, userId, applied, cancellationToken);
    }

    public async Task<ModelReactionOutcome> RemoveReactionAsync(Guid modelId, ClaimsPrincipal user, ModelReactionType type, CancellationToken cancellationToken = default)
    {
        if (!TryResolveUserId(user, out var userId))
        {
            return ModelReactionOutcome.Unauthorized;
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

        var existing = await repository.GetUserReactionAsync(modelId, userId, cancellationToken);
        var applied = existing == type && await repository.TryRemoveAsync(modelId, userId, type, cancellationToken);

        if (applied)
        {
            logger.LogInformation("User {UserId} removed {Reaction} from model {ModelId}", userId, type, modelId);
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
