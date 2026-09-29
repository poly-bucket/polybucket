using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.RemoveTagFromModel.Domain;
using Tag = PolyBucket.Api.Features.Models.AddTagToModel.Domain.Tag;
using PolyBucket.Api.Features.Models.RemoveTagFromModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RemoveTagFromModel;

public class RemoveTagFromModelServiceTests
{
    private readonly Mock<IRemoveTagFromModelRepository> _mockRepository;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<RemoveTagFromModelService>> _mockLogger;
    private readonly RemoveTagFromModelService _service;

    public RemoveTagFromModelServiceTests()
    {
        _mockRepository = new Mock<IRemoveTagFromModelRepository>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<RemoveTagFromModelService>>();
        _service = new RemoveTagFromModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When removing a linked tag as the owner, the remove tag service unlinks the tag.")]
    public async Task RemoveTagFromModelAsync_WithLinkedTag_ShouldUnlinkTag()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        model.Tags.Add(new Tag { Id = tagId, Name = "printable" });
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.RemoveTagFromModelAsync(modelId, tagId, user, cancellationToken);

        // Assert
        model.Tags.ShouldBeEmpty();
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact(DisplayName = "When removing a tag that is not linked, the remove tag service throws a ModelNotFoundException.")]
    public async Task RemoveTagFromModelAsync_WithMissingLink_ShouldThrowModelNotFoundException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<ModelNotFoundException>(async () =>
            await _service.RemoveTagFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a tag from a model that does not exist, the remove tag service throws a ModelNotFoundException.")]
    public async Task RemoveTagFromModelAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync((Model?)null);

        // Act & Assert
        await Should.ThrowAsync<ModelNotFoundException>(async () =>
            await _service.RemoveTagFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a tag from a deleted model, the remove tag service throws a ValidationException.")]
    public async Task RemoveTagFromModelAsync_WithDeletedModel_ShouldThrowValidationException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        model.DeletedAt = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(async () =>
            await _service.RemoveTagFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a tag as a non-owner, the remove tag service throws an UnauthorizedAccessException.")]
    public async Task RemoveTagFromModelAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await _service.RemoveTagFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a tag as a user with MODEL_EDIT_ANY, the remove tag service unlinks the tag.")]
    public async Task RemoveTagFromModelAsync_WithEditAnyPermission_ShouldUnlinkTag()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        model.Tags.Add(new Tag { Id = tagId, Name = "printable" });
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.RemoveTagFromModelAsync(modelId, tagId, user, cancellationToken);

        // Assert
        model.Tags.ShouldBeEmpty();
    }

    private static ClaimsPrincipal CreateTestUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static Model CreateTestModel(Guid modelId, Guid authorId)
    {
        return new Model
        {
            Id = modelId,
            Name = "Original Name",
            AuthorId = authorId
        };
    }
}
