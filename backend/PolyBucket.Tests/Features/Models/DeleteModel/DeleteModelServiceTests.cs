using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.DeleteModel.Domain;
using PolyBucket.Api.Features.Models.DeleteModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteModel
{
    public class DeleteModelServiceTests
    {
        private readonly Mock<IDeleteModelRepository> _mockRepository;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<ILogger<DeleteModelService>> _mockLogger;
        private readonly DeleteModelService _service;

        public DeleteModelServiceTests()
        {
            _mockRepository = new Mock<IDeleteModelRepository>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<DeleteModelService>>();
            _service = new DeleteModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task DeleteModelAsync_WithValidRequest_ShouldDeleteModel()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_DELETE_ANY permission

            _mockRepository.Setup(x => x.DeleteModelAsync(It.IsAny<Model>(), cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteModelAsync(modelId, user, cancellationToken);

            // Assert
            _mockRepository.Verify(x => x.DeleteModelAsync(It.IsAny<Model>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task DeleteModelAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync((Model?)null);

            // Act & Assert
            await Should.ThrowAsync<ModelNotFoundException>(async () =>
                await _service.DeleteModelAsync(modelId, user, cancellationToken));
        }

        [Fact]
        public async Task DeleteModelAsync_WithAlreadyDeletedModel_ShouldThrowValidationException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var deletedModel = CreateTestModel(modelId, userId);
            deletedModel.DeletedAt = DateTime.UtcNow;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(deletedModel);

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.DeleteModelAsync(modelId, user, cancellationToken));
        }

        [Fact]
        public async Task DeleteModelAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, otherUserId); // Model belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_DELETE_ANY permission

            // Act & Assert
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
                await _service.DeleteModelAsync(modelId, user, cancellationToken));
        }

        [Fact]
        public async Task DeleteModelAsync_WithAdminPermission_ShouldAllowDelete()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, otherUserId); // Model belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true); // User has MODEL_DELETE_ANY permission

            _mockRepository.Setup(x => x.DeleteModelAsync(It.IsAny<Model>(), cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteModelAsync(modelId, user, cancellationToken);

            // Assert
            _mockRepository.Verify(x => x.DeleteModelAsync(It.IsAny<Model>(), cancellationToken), Times.Once);
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
                Name = "Test Model",
                Description = "Test Description",
                AuthorId = authorId
            };
        }
    }
} 