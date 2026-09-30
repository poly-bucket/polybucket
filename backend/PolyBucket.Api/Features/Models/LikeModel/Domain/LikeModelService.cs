using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.DeleteModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Repository;
using PolyBucket.Api.Features.Notifications.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Domain;

public class LikeModelService(
    ILikeModelRepository repository,
    IPermissionService permissionService,
    INotificationPublisher notificationPublisher,
    ILogger<LikeModelService> logger) : ILikeModelService
{
    public async Task LikeModelAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId(user);

        if (!await repository.IsModelLikesEnabledAsync(cancellationToken))
        {
            throw new UnauthorizedAccessException("Model likes are disabled");
        }

        var model = await repository.GetModelByIdAsync(modelId, cancellationToken);
        if (model == null)
        {
            throw new ModelNotFoundException($"Model with ID {modelId} not found");
        }

        if (!await CanUserAccessModelAsync(model, userId, cancellationToken))
        {
            throw new UnauthorizedAccessException("You do not have permission to like this model");
        }

        var existingLike = await repository.FindLikeAsync(modelId, userId, cancellationToken);
        if (existingLike != null && existingLike.DeletedAt == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (existingLike != null)
        {
            existingLike.DeletedAt = null;
            existingLike.DeletedById = null;
            existingLike.UpdatedAt = now;
            existingLike.UpdatedById = userId;
        }
        else
        {
            repository.AddLike(new Like
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                UserId = userId,
                CreatedAt = now,
                CreatedById = userId,
                UpdatedAt = now,
                UpdatedById = userId
            });
        }

        model.Likes++;
        model.UpdatedAt = now;
        model.UpdatedById = userId;

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

        await repository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} liked model {ModelId}", userId, modelId);
    }

    public async Task UnlikeModelAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId(user);

        var model = await repository.GetModelByIdAsync(modelId, cancellationToken);
        if (model == null)
        {
            throw new ModelNotFoundException($"Model with ID {modelId} not found");
        }

        var existingLike = await repository.FindLikeAsync(modelId, userId, cancellationToken);
        if (existingLike == null || existingLike.DeletedAt != null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        existingLike.DeletedAt = now;
        existingLike.DeletedById = userId;
        existingLike.UpdatedAt = now;
        existingLike.UpdatedById = userId;

        model.Likes = Math.Max(0, model.Likes - 1);
        model.UpdatedAt = now;
        model.UpdatedById = userId;

        await repository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} unliked model {ModelId}", userId, modelId);
    }

    private static Guid ResolveUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new ValidationException("Invalid authentication token");
        }

        return userId;
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
