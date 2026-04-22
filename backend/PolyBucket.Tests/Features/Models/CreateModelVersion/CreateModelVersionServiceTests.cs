using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using PolyBucket.Api.Features.Models.CreateModelVersion.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.CreateModelVersion
{
    public class CreateModelVersionServiceTests
    {
        private readonly Mock<ICreateModelVersionRepository> _mockRepository;
        private readonly Mock<IStorageService> _mockStorage;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<ILogger<CreateModelVersionService>> _mockLogger;
        private readonly CreateModelVersionService _service;

        public CreateModelVersionServiceTests()
        {
            _mockRepository = new Mock<ICreateModelVersionRepository>();
            _mockStorage = new Mock<IStorageService>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<CreateModelVersionService>>();
            _service = new CreateModelVersionService(_mockRepository.Object, _mockStorage.Object, _mockPermissionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithValidRequest_ShouldCreateVersion()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateModelVersionRequest
            {
                Name = "Version 2.0",
                Notes = "Updated version",
                Files = CreateTestFiles()
            };

            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_EDIT_ANY permission

            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("https://storage.example.com/test-file.stl");

            _mockRepository.Setup(x => x.CreateModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken))
                .ReturnsAsync((ModelVersion version, CancellationToken ct) => version);

            // Act
            var result = await _service.CreateModelVersionAsync(modelId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.ModelVersion.ShouldNotBeNull();
            result.ModelVersion.Name.ShouldBe("Version 2.0");
            result.ModelVersion.Notes.ShouldBe("Updated version");
            
            _mockRepository.Verify(x => x.CreateModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken), Times.Once);
            _mockStorage.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithModelNotFound_ShouldThrowModelNotFoundException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateModelVersionRequest { Name = "Version 2.0", Files = CreateTestFiles() };
            var user = CreateTestUser(userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync((Model?)null);

            // Act & Assert
            await Should.ThrowAsync<ModelNotFoundException>(async () =>
                await _service.CreateModelVersionAsync(modelId, request, user, cancellationToken));
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithDeletedModel_ShouldThrowValidationException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateModelVersionRequest { Name = "Version 2.0", Files = CreateTestFiles() };
            var user = CreateTestUser(userId);
            var deletedModel = CreateTestModel(modelId, userId);
            deletedModel.DeletedAt = DateTime.UtcNow;
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(deletedModel);

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelVersionAsync(modelId, request, user, cancellationToken));
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new CreateModelVersionRequest { Name = "Version 2.0", Files = CreateTestFiles() };
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, otherUserId); // Model belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_EDIT_ANY permission

            // Act & Assert
            await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
                await _service.CreateModelVersionAsync(modelId, request, user, cancellationToken));
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithAdminPermission_ShouldAllowCreate()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new CreateModelVersionRequest { Name = "Version 2.0", Files = CreateTestFiles() };
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, otherUserId); // Model belongs to different user
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true); // User has MODEL_EDIT_ANY permission

            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("https://storage.example.com/test-file.stl");

            _mockRepository.Setup(x => x.CreateModelVersionAsync(It.IsAny<ModelVersion>(), cancellationToken))
                .ReturnsAsync((ModelVersion version, CancellationToken ct) => version);

            // Act
            var result = await _service.CreateModelVersionAsync(modelId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.ModelVersion.Name.ShouldBe("Version 2.0");
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithoutFiles_ShouldThrowValidationException()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateModelVersionRequest { Name = "Version 2.0", Files = new IFormFile[0] };
            var user = CreateTestUser(userId);
            var model = CreateTestModel(modelId, userId);
            var cancellationToken = CancellationToken.None;

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId, cancellationToken))
                .ReturnsAsync(model);

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelVersionAsync(modelId, request, user, cancellationToken));
        }

        private static IFormFile[] CreateTestFiles()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.stl");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("application/octet-stream");
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

            return new[] { mockFile.Object };
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