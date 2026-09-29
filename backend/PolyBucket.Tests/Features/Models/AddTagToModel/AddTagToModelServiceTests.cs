using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.AddTagToModel;

public class AddTagToModelServiceTests
{
    private readonly Mock<IAddTagToModelRepository> _mockRepository;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<AddTagToModelService>> _mockLogger;
    private readonly AddTagToModelService _service;

    public AddTagToModelServiceTests()
    {
        _mockRepository = new Mock<IAddTagToModelRepository>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<AddTagToModelService>>();
        _service = new AddTagToModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When adding a tag as the owner, the add tag service links a new tag.")]
    public async Task AddTagToModelAsync_WithValidRequest_ShouldLinkTag()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.GetActiveTagByNameAsync("printable", cancellationToken)).ReturnsAsync((Tag?)null);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.AddTagToModelAsync(modelId, " printable ", user, cancellationToken);

        // Assert
        model.Tags.ShouldHaveSingleItem().Name.ShouldBe("printable");
        _mockRepository.Verify(x => x.AddTag(It.Is<Tag>(t => t.Name == "printable")), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact(DisplayName = "When adding a tag that already exists, the add tag service reuses that tag.")]
    public async Task AddTagToModelAsync_WithExistingTag_ShouldReuseTag()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        var existingTag = new Tag { Id = Guid.NewGuid(), Name = "PLA" };
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.GetActiveTagByNameAsync("pla", cancellationToken)).ReturnsAsync(existingTag);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.AddTagToModelAsync(modelId, "pla", user, cancellationToken);

        // Assert
        model.Tags.ShouldHaveSingleItem().Id.ShouldBe(existingTag.Id);
        _mockRepository.Verify(x => x.AddTag(It.IsAny<Tag>()), Times.Never);
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact(DisplayName = "When adding a tag with a blank name, the add tag service throws a ValidationException.")]
    public async Task AddTagToModelAsync_WithBlankName_ShouldThrowValidationException()
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
        await Should.ThrowAsync<ValidationException>(async () =>
            await _service.AddTagToModelAsync(modelId, "   ", user, cancellationToken));
        _mockRepository.Verify(x => x.AddTag(It.IsAny<Tag>()), Times.Never);
    }

    [Fact(DisplayName = "When adding a tag to a model that does not exist, the add tag service throws a ModelNotFoundException.")]
    public async Task AddTagToModelAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync((Model?)null);

        // Act & Assert
        await Should.ThrowAsync<ModelNotFoundException>(async () =>
            await _service.AddTagToModelAsync(modelId, "printable", user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a tag to a deleted model, the add tag service throws a ValidationException.")]
    public async Task AddTagToModelAsync_WithDeletedModel_ShouldThrowValidationException()
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
            await _service.AddTagToModelAsync(modelId, "printable", user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a tag as a non-owner, the add tag service throws an UnauthorizedAccessException.")]
    public async Task AddTagToModelAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
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
            await _service.AddTagToModelAsync(modelId, "printable", user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a tag as a user with MODEL_EDIT_ANY, the add tag service links the tag.")]
    public async Task AddTagToModelAsync_WithEditAnyPermission_ShouldLinkTag()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetActiveTagByNameAsync("printable", cancellationToken)).ReturnsAsync((Tag?)null);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.AddTagToModelAsync(modelId, "printable", user, cancellationToken);

        // Assert
        model.Tags.ShouldHaveSingleItem().Name.ShouldBe("printable");
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
