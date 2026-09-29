using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.AddCategoryToModel;

public class AddCategoryToModelServiceTests
{
    private readonly Mock<IAddCategoryToModelRepository> _mockRepository;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<AddCategoryToModelService>> _mockLogger;
    private readonly AddCategoryToModelService _service;

    public AddCategoryToModelServiceTests()
    {
        _mockRepository = new Mock<IAddCategoryToModelRepository>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<AddCategoryToModelService>>();
        _service = new AddCategoryToModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When adding a category as the owner, the add category service links the category.")]
    public async Task AddCategoryToModelAsync_WithValidRequest_ShouldLinkCategory()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        var category = new Category { Id = categoryId, Name = "Toys" };
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.GetActiveCategoryAsync(categoryId, cancellationToken)).ReturnsAsync(category);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.AddCategoryToModelAsync(modelId, categoryId, user, cancellationToken);

        // Assert
        model.Categories.ShouldHaveSingleItem().Id.ShouldBe(categoryId);
        _mockRepository.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact(DisplayName = "When adding a category that does not exist, the add category service throws a ModelNotFoundException.")]
    public async Task AddCategoryToModelAsync_WithMissingCategory_ShouldThrowModelNotFoundException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.GetActiveCategoryAsync(categoryId, cancellationToken)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Should.ThrowAsync<ModelNotFoundException>(async () =>
            await _service.AddCategoryToModelAsync(modelId, categoryId, user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a category to a model that does not exist, the add category service throws a ModelNotFoundException.")]
    public async Task AddCategoryToModelAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync((Model?)null);

        // Act & Assert
        await Should.ThrowAsync<ModelNotFoundException>(async () =>
            await _service.AddCategoryToModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a category to a deleted model, the add category service throws a ValidationException.")]
    public async Task AddCategoryToModelAsync_WithDeletedModel_ShouldThrowValidationException()
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
            await _service.AddCategoryToModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a category as a non-owner, the add category service throws an UnauthorizedAccessException.")]
    public async Task AddCategoryToModelAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
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
            await _service.AddCategoryToModelAsync(modelId, Guid.NewGuid(), user, cancellationToken));
    }

    [Fact(DisplayName = "When adding a category as a user with MODEL_EDIT_ANY, the add category service links the category.")]
    public async Task AddCategoryToModelAsync_WithEditAnyPermission_ShouldLinkCategory()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var model = CreateTestModel(modelId, Guid.NewGuid());
        var category = new Category { Id = categoryId, Name = "Toys" };
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(x => x.GetModelAsync(modelId, cancellationToken)).ReturnsAsync(model);
        _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY)).ReturnsAsync(true);
        _mockRepository.Setup(x => x.GetActiveCategoryAsync(categoryId, cancellationToken)).ReturnsAsync(category);
        _mockRepository.Setup(x => x.SaveChangesAsync(cancellationToken)).Returns(Task.CompletedTask);

        // Act
        await _service.AddCategoryToModelAsync(modelId, categoryId, user, cancellationToken);

        // Assert
        model.Categories.ShouldHaveSingleItem().Id.ShouldBe(categoryId);
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
