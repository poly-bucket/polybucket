using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using PolyBucket.Api.Features.Models.UpdateModel.Domain;
using PolyBucket.Api.Features.Models.UpdateModel.Http;
using PolyBucket.Api.Features.Models.UpdateModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.UpdateModel
{
    public class UpdateModelServiceTests
    {
        private readonly Mock<IUpdateModelRepository> _mockRepository;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<ILogger<UpdateModelService>> _mockLogger;
        private readonly UpdateModelService _service;

        public UpdateModelServiceTests()
        {
            _mockRepository = new Mock<IUpdateModelRepository>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<UpdateModelService>>();
            _service = new UpdateModelService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpdateModelAsync_WithValidRequest_ShouldUpdateModel()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateModelRequest
            {
                Name = "Updated Model Name",
                Description = "Updated Description",
                License = LicenseTypes.MIT,
                Privacy = PrivacySettings.Public
            };

            var user = CreateTestUser(userId);
            var existingModel = CreateTestModel(modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(existingModel);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_EDIT_ANY permission

            _mockRepository.Setup(x => x.UpdateModelAsync(It.IsAny<Model>(), cancellationToken))
                .ReturnsAsync((Model model, CancellationToken ct) => model);

            // Act
            var result = await _service.UpdateModelAsync(modelId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Model.ShouldNotBeNull();
            result.Model.Name.ShouldBe("Updated Model Name");
            result.Model.Description.ShouldBe("Updated Description");
            result.Model.License.ShouldBe(LicenseTypes.MIT);
            result.Model.Privacy.ShouldBe(PrivacySettings.Public);
            
            _mockRepository.Verify(x => x.UpdateModelAsync(It.IsAny<Model>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateModelAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateModelRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync((Model?)null);

            // Act & Assert
            await Should.ThrowAsync<ModelNotFoundException>(async () =>
                await _service.UpdateModelAsync(modelId, request, user, cancellationToken));
        }

        [Fact]
        public async Task UpdateModelAsync_WithDeletedModel_ShouldThrowValidationException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateModelRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var deletedModel = CreateTestModel(modelId, userId);
            deletedModel.DeletedAt = DateTime.UtcNow;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(deletedModel);

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.UpdateModelAsync(modelId, request, user, cancellationToken));
        }

        [Fact]
        public async Task UpdateModelAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new UpdateModelRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, otherUserId); // Model belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_EDIT_ANY permission

            // Act & Assert
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
                await _service.UpdateModelAsync(modelId, request, user, cancellationToken));
        }

        [Fact]
        public async Task UpdateModelAsync_WithAdminPermission_ShouldAllowUpdate()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new UpdateModelRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, otherUserId); // Model belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true); // User has MODEL_EDIT_ANY permission

            _mockRepository.Setup(x => x.UpdateModelAsync(It.IsAny<Model>(), cancellationToken))
                .ReturnsAsync((Model m, CancellationToken ct) => m);

            // Act
            var result = await _service.UpdateModelAsync(modelId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Model.Name.ShouldBe("Updated Name");
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
                Description = "Original Description",
                AuthorId = authorId
            };
        }
    }
} 