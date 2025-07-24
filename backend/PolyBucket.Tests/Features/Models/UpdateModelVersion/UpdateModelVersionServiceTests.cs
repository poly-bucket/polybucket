using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Domain;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Http;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.UpdateModelVersion
{
    public class UpdateModelVersionServiceTests
    {
        private readonly Mock<IUpdateModelVersionRepository> _mockRepository;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<ILogger<UpdateModelVersionService>> _mockLogger;
        private readonly UpdateModelVersionService _service;

        public UpdateModelVersionServiceTests()
        {
            _mockRepository = new Mock<IUpdateModelVersionRepository>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<UpdateModelVersionService>>();
            _service = new UpdateModelVersionService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpdateModelVersionAsync_WithValidRequest_ShouldUpdateVersion()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateModelVersionRequest
            {
                Name = "Updated Version Name",
                Notes = "Updated notes"
            };

            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_EDIT_ANY permission

            _mockRepository.Setup(x => x.UpdateModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken))
                .ReturnsAsync((ModelVersion version, CancellationToken ct) => version);

            // Act
            var result = await _service.UpdateModelVersionAsync(modelId, versionId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.ModelVersion.ShouldNotBeNull();
            result.ModelVersion.Name.ShouldBe("Updated Version Name");
            result.ModelVersion.Notes.ShouldBe("Updated notes");
            
            _mockRepository.Verify(x => x.UpdateModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateModelVersionAsync_WithModelVersionNotFound_ShouldThrowModelVersionNotFoundException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateModelVersionRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync((ModelVersion?)null);

            // Act & Assert
            await Should.ThrowAsync<ModelVersionNotFoundException>(async () =>
                await _service.UpdateModelVersionAsync(modelId, versionId, request, user, cancellationToken));
        }

        [Fact]
        public async Task UpdateModelVersionAsync_WithWrongModelId_ShouldThrowValidationException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var wrongModelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new UpdateModelVersionRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.UpdateModelVersionAsync(wrongModelId, versionId, request, user, cancellationToken));
        }

        [Fact]
        public async Task UpdateModelVersionAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new UpdateModelVersionRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, otherUserId); // Version belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_EDIT_ANY permission

            // Act & Assert
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
                await _service.UpdateModelVersionAsync(modelId, versionId, request, user, cancellationToken));
        }

        [Fact]
        public async Task UpdateModelVersionAsync_WithAdminPermission_ShouldAllowUpdate()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new UpdateModelVersionRequest { Name = "Updated Name" };
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, otherUserId); // Version belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true); // User has MODEL_EDIT_ANY permission

            _mockRepository.Setup(x => x.UpdateModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken))
                .ReturnsAsync((ModelVersion version, CancellationToken ct) => version);

            // Act
            var result = await _service.UpdateModelVersionAsync(modelId, versionId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.ModelVersion.Name.ShouldBe("Updated Name");
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

        private static ModelVersion CreateTestModelVersion(Guid versionId, Guid modelId, Guid authorId)
        {
            return new ModelVersion
            {
                Id = versionId,
                Name = "Original Version Name",
                Notes = "Original notes",
                ModelId = modelId,
                VersionNumber = 1,
                Model = new Model
                {
                    Id = modelId,
                    Name = "Test Model",
                    AuthorId = authorId
                }
            };
        }
    }
} 