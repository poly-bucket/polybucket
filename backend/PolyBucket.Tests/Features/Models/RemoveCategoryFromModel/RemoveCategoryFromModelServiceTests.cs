using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Domain;
using Category = PolyBucket.Api.Features.Models.AddCategoryToModel.Domain.Category;
using PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RemoveCategoryFromModel;

public class RemoveCategoryFromModelServiceTests
{
    private readonly Mock<IRemoveCategoryFromModelRepository> _mockRepository;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<RemoveCategoryFromModelService>> _mockLogger;
    private readonly RemoveCategoryFromModelService _service;

    public RemoveCategoryFromModelServiceTests()
    {
        _mockRepository = new Mock<IRemoveCategoryFromModelRepository>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<RemoveCategoryFromModelService>>();
        _service = new RemoveCategoryFromModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When removing a linked category as the owner, the remove category service unlinks the category.")]
    public async Task RemoveCategoryFromModelAsync_WithLinkedCategory_ShouldUnlinkCategory()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        model.Categories.Add(new Category { Id = categoryId, Name = "Toys" });
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.RemoveCategoryFromModelAsync(modelId, categoryId, user, cancellationToken);

        // Assert
        model.Categories.ShouldBeEmpty();
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact(DisplayName = "When removing a category that is not linked, the remove category service throws a ModelNotFoundException.")]
    public async Task RemoveCategoryFromModelAsync_WithMissingLink_ShouldThrowModelNotFoundException()
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
            await _service.RemoveCategoryFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a category from a model that does not exist, the remove category service throws a ModelNotFoundException.")]
    public async Task RemoveCategoryFromModelAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync((Model?)null);

        // Act & Assert
        await Should.ThrowAsync<ModelNotFoundException>(async () =>
            await _service.RemoveCategoryFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a category from a deleted model, the remove category service throws a ValidationException.")]
    public async Task RemoveCategoryFromModelAsync_WithDeletedModel_ShouldThrowValidationException()
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
            await _service.RemoveCategoryFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a category as a non-owner, the remove category service throws an UnauthorizedAccessException.")]
    public async Task RemoveCategoryFromModelAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
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
            await _service.RemoveCategoryFromModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When removing a category as a user with MODEL_EDIT_ANY, the remove category service unlinks the category.")]
    public async Task RemoveCategoryFromModelAsync_WithEditAnyPermission_ShouldUnlinkCategory()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        model.Categories.Add(new Category { Id = categoryId, Name = "Toys" });
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.RemoveCategoryFromModelAsync(modelId, categoryId, user, cancellationToken);

        // Assert
        model.Categories.ShouldBeEmpty();
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
