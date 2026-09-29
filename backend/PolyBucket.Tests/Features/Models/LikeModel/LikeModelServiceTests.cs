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
using PolyBucket.Api.Features.Models.DeleteModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.LikeModel;

public class LikeModelServiceTests
{
    private readonly Mock<ILikeModelRepository> _mockRepository;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<LikeModelService>> _mockLogger;
    private readonly LikeModelService _service;

    public LikeModelServiceTests()
    {
        _mockRepository = new Mock<ILikeModelRepository>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<LikeModelService>>();
        _service = new LikeModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When liking a model with a valid request, the like model service increments the like count.")]
    public async Task LikeModelAsync_WithValidRequest_IncrementsLikeCount()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, authorId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.IsModelLikesEnabledAsync(cancellationToken)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockRepository.Setup(x => x.FindLikeAsync(modelId, userId, cancellationToken)).ReturnsAsync((Api.Features.Models.LikeModel.Domain.Like?)null);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        await _service.LikeModelAsync(modelId, user, cancellationToken);

        model.Likes.ShouldBe(1);
        _mockRepository.Verify(x => x.AddLike(It.IsAny<Api.Features.Models.LikeModel.Domain.Like>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact(DisplayName = "When liking a model that is already liked, the like model service is idempotent.")]
    public async Task LikeModelAsync_WhenAlreadyLiked_DoesNotIncrementAgain()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        var existingLike = new Api.Features.Models.LikeModel.Domain.Like
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            UpdatedById = userId
        };
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.IsModelLikesEnabledAsync(cancellationToken)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockRepository.Setup(x => x.FindLikeAsync(modelId, userId, cancellationToken)).ReturnsAsync(existingLike);

        await _service.LikeModelAsync(modelId, user, cancellationToken);

        model.Likes.ShouldBe(0);
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Never);
    }

    [Fact(DisplayName = "When liking a model that does not exist, the like model service throws ModelNotFoundException.")]
    public async Task LikeModelAsync_WithMissingModel_ThrowsModelNotFoundException()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.IsModelLikesEnabledAsync(cancellationToken)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken)).ReturnsAsync((Model?)null);

        await Should.ThrowAsync<ModelNotFoundException>(() =>
            _service.LikeModelAsync(modelId, user, cancellationToken));
    }

    [Fact(DisplayName = "When model likes are disabled, the like model service throws UnauthorizedAccessException.")]
    public async Task LikeModelAsync_WhenLikesDisabled_ThrowsUnauthorizedAccessException()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.IsModelLikesEnabledAsync(cancellationToken)).ReturnsAsync(false);

        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            _service.LikeModelAsync(modelId, user, cancellationToken));
    }

    [Fact(DisplayName = "When unliking a liked model, the like model service decrements the like count.")]
    public async Task UnlikeModelAsync_WithExistingLike_DecrementsLikeCount()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        model.Likes = 1;
        var existingLike = new Api.Features.Models.LikeModel.Domain.Like
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            UpdatedById = userId
        };
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockRepository.Setup(x => x.FindLikeAsync(modelId, userId, cancellationToken)).ReturnsAsync(existingLike);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        await _service.UnlikeModelAsync(modelId, user, cancellationToken);

        model.Likes.ShouldBe(0);
        existingLike.DeletedAt.ShouldNotBeNull();
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static ClaimsPrincipal CreateTestUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "test@example.com")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static Model CreateTestModel(Guid modelId, Guid authorId)
    {
        return new Model
        {
            Id = modelId,
            Name = "Test Model",
            Description = "Test Description",
            AuthorId = authorId,
            Privacy = PrivacySettings.Public,
            Likes = 0
        };
    }
}
