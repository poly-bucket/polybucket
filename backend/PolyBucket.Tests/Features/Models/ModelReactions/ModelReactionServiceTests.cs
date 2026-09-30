using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using PolyBucket.Api.Features.Models.ModelReactions.Repository;
using PolyBucket.Api.Features.Notifications.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.ModelReactions;

public class ModelReactionServiceTests
{
    private readonly Mock<IModelReactionRepository> _repository = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly Mock<ILogger<ModelReactionService>> _logger = new();
    private readonly Mock<INotificationPublisher> _publisher = new();
    private readonly ModelReactionService _service;

    public ModelReactionServiceTests()
    {
        _service = new ModelReactionService(_repository.Object, _permissionService.Object, _publisher.Object, _logger.Object);
    }

    [Fact(DisplayName = "When a user likes a model for the first time, a like is applied and a notification is published.")]
    public async Task React_FirstLike_PublishesNotification()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var user = CreateUser(userId);
        var model = CreateModel(modelId, authorId);
        NotificationRequest? published = null;

        _repository.Setup(r => r.IsReactionsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetModelByIdAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(model);
        _repository.SetupSequence(r => r.GetUserReactionAsync(modelId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelReactionType?)null)
            .ReturnsAsync(ModelReactionType.Like);
        _repository.Setup(r => r.TryAddAsync(modelId, userId, ModelReactionType.Like, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetCountsAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync((1, 0));
        _publisher.Setup(p => p.PublishAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationRequest, CancellationToken>((r, _) => published = r)
            .ReturnsAsync(true);

        var outcome = await _service.ReactAsync(modelId, user, ModelReactionType.Like, CancellationToken.None);

        outcome.Change.ShouldBe(ModelReactionChange.Applied);
        published.ShouldNotBeNull();
        published!.Type.ShouldBe(NotificationType.ModelLiked);
    }

    [Fact(DisplayName = "When the model author reacts to their own model, the outcome is Forbidden.")]
    public async Task React_SelfReaction_ReturnsForbidden()
    {
        var modelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var user = CreateUser(authorId);
        var model = CreateModel(modelId, authorId);

        _repository.Setup(r => r.IsReactionsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetModelByIdAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var outcome = await _service.ReactAsync(modelId, user, ModelReactionType.Like, CancellationToken.None);

        outcome.Change.ShouldBe(ModelReactionChange.Forbidden);
        _repository.Verify(r => r.TryAddAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ModelReactionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When reactions are disabled, ReactAsync returns Disabled.")]
    public async Task React_WhenDisabled_ReturnsDisabled()
    {
        var user = CreateUser(Guid.NewGuid());
        _repository.Setup(r => r.IsReactionsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var outcome = await _service.ReactAsync(Guid.NewGuid(), user, ModelReactionType.Like, CancellationToken.None);

        outcome.Change.ShouldBe(ModelReactionChange.Disabled);
    }

    [Fact(DisplayName = "When switching from like to dislike, no like notification is published.")]
    public async Task React_SwitchLikeToDislike_DoesNotPublishLikeNotification()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        var model = CreateModel(modelId, Guid.NewGuid());

        _repository.Setup(r => r.IsReactionsEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetModelByIdAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync(model);
        _repository.SetupSequence(r => r.GetUserReactionAsync(modelId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModelReactionType.Like)
            .ReturnsAsync(ModelReactionType.Dislike);
        _repository.Setup(r => r.TrySwitchAsync(modelId, userId, ModelReactionType.Like, ModelReactionType.Dislike, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetCountsAsync(modelId, It.IsAny<CancellationToken>())).ReturnsAsync((0, 1));

        await _service.ReactAsync(modelId, user, ModelReactionType.Dislike, CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ClaimsPrincipal CreateUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "test@example.com")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static Model CreateModel(Guid modelId, Guid authorId) =>
        new()
        {
            Id = modelId,
            Name = "Test",
            Description = "Desc",
            AuthorId = authorId,
            Privacy = PrivacySettings.Public
        };
}
