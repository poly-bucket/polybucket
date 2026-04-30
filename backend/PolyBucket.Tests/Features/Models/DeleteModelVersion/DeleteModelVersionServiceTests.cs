using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Domain;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteModelVersion
{
    public class DeleteModelVersionServiceTests
    {
        private readonly Mock<IDeleteModelVersionRepository> _mockRepository;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<ILogger<DeleteModelVersionService>> _mockLogger;
        private readonly DeleteModelVersionService _service;

        public DeleteModelVersionServiceTests()
        {
            _mockRepository = new Mock<IDeleteModelVersionRepository>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<DeleteModelVersionService>>();
            _service = new DeleteModelVersionService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object);
        }

        [Fact(DisplayName = "When deleting a model version with a valid request, the delete model version service deletes the version.")]
        public async Task DeleteModelVersionAsync_WithValidRequest_ShouldDeleteVersion()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_DELETE_ANY permission

            _mockRepository.Setup(x => x.DeleteModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteModelVersionAsync(modelId, versionId, user, cancellationToken);

            // Assert
            _mockRepository.Verify(x => x.DeleteModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken), Times.Once);
        }

        [Fact(DisplayName = "When deleting a model version that does not exist, the delete model version service throws a ModelVersionNotFoundException.")]
        public async Task DeleteModelVersionAsync_WithModelVersionNotFound_ShouldThrowModelVersionNotFoundException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync((ModelVersion?)null);

            // Act & Assert
            await Should.ThrowAsync<ModelVersionNotFoundException>(async () =>
                await _service.DeleteModelVersionAsync(modelId, versionId, user, cancellationToken));
        }

        [Fact(DisplayName = "When deleting a model version under the wrong model id, the delete model version service throws a ModelVersionNotFoundException.")]
        public async Task DeleteModelVersionAsync_WithWrongModelId_ShouldThrowModelVersionNotFoundException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var wrongModelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            // Act & Assert
            await Should.ThrowAsync<ModelVersionNotFoundException>(async () =>
                await _service.DeleteModelVersionAsync(wrongModelId, versionId, user, cancellationToken));
        }

        [Fact(DisplayName = "When deleting a model version as a user without permission, the delete model version service throws an UnauthorizedAccessException.")]
        public async Task DeleteModelVersionAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, otherUserId); // Version belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_DELETE_ANY permission

            // Act & Assert
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
                await _service.DeleteModelVersionAsync(modelId, versionId, user, cancellationToken));
        }

        [Fact(DisplayName = "When deleting a model version as a user with the MODEL_DELETE_ANY permission, the delete model version service allows the deletion.")]
        public async Task DeleteModelVersionAsync_WithAdminPermission_ShouldAllowDelete()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var user = CreateTestUser(userId);
            var modelVersion = CreateTestModelVersion(versionId, modelId, otherUserId); // Version belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelVersionAsync(modelId, versionId, cancellationToken))
                .ReturnsAsync(modelVersion);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true); // User has MODEL_DELETE_ANY permission

            _mockRepository.Setup(x => x.DeleteModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteModelVersionAsync(modelId, versionId, user, cancellationToken);

            // Assert
            _mockRepository.Verify(x => x.DeleteModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken), Times.Once);
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
                Name = "Test Version",
                Notes = "Test notes",
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