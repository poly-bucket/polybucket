using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using PolyBucket.Api.Features.Models.CreateModelVersion.Repository;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.CreateModelVersion
{
    public class CreateModelVersionServiceTests
    {
        private readonly Mock<ICreateModelVersionRepository> _mockRepository;
        private readonly Mock<PolyBucket.Api.Common.Storage.IStorageService> _mockStorage;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<IModelPreviewQueue> _mockPreviewQueue = new();
        private readonly Mock<ILogger<CreateModelVersionService>> _mockLogger;
        private readonly CreateModelVersionService _service;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _modelId = Guid.NewGuid();

        public CreateModelVersionServiceTests()
        {
            _mockRepository = new Mock<ICreateModelVersionRepository>();
            _mockStorage = new Mock<PolyBucket.Api.Common.Storage.IStorageService>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<CreateModelVersionService>>();
            _service = new CreateModelVersionService(
                _mockRepository.Object,
                _mockStorage.Object,
                _mockPermissionService.Object,
                _mockPreviewQueue.Object,
                _mockLogger.Object);

            _mockRepository
                .Setup(x => x.GetModelByIdAsync(_modelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Model { Id = _modelId, AuthorId = _userId });
            _mockRepository
                .Setup(x => x.GetNextVersionNumberAsync(_modelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);
            _mockRepository
                .Setup(x => x.CreateModelVersionAsync(It.IsAny<ModelVersion>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ModelVersion version, CancellationToken ct) => version);
            _mockStorage
                .Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://storage.example.com/test-file");
        }

        [Fact(DisplayName = "When creating a model version with only 3D files, the create model version service leaves the thumbnail unset.")]
        public async Task CreateModelVersionAsync_WithOnly3DFiles_ShouldLeaveThumbnailNull()
        {
            // Arrange
            var request = new CreateModelVersionRequest
            {
                Name = "2.0",
                Notes = "Notes",
                Files = Create3DFiles()
            };

            // Act
            var result = await _service.CreateModelVersionAsync(_modelId, request, CreateUser(), CancellationToken.None);

            // Assert
            result.ModelVersion.ShouldNotBeNull();
            result.ModelVersion.ThumbnailUrl.ShouldBeNull();
        }

        [Fact(DisplayName = "When a model version is created, a background preview is queued for the model.")]
        public async Task CreateModelVersionAsync_QueuesPreview()
        {
            // Arrange
            var request = new CreateModelVersionRequest { Name = "2.0", Notes = "Notes", Files = Create3DFiles() };

            // Act
            await _service.CreateModelVersionAsync(_modelId, request, CreateUser(), CancellationToken.None);

            // Assert
            _mockPreviewQueue.Verify(q => q.EnqueueForNewContentAsync(_modelId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When creating a model version that includes an image file, the create model version service uses the image as the thumbnail.")]
        public async Task CreateModelVersionAsync_WithImageFile_ShouldSetThumbnailToImage()
        {
            // Arrange
            var request = new CreateModelVersionRequest
            {
                Name = "2.0",
                Notes = "Notes",
                Files = Create3DAndImageFiles()
            };

            // Act
            var result = await _service.CreateModelVersionAsync(_modelId, request, CreateUser(), CancellationToken.None);

            // Assert
            result.ModelVersion.ShouldNotBeNull();
            result.ModelVersion.ThumbnailUrl.ShouldNotBeNull();
            result.ModelVersion.ThumbnailUrl!.ShouldEndWith("preview.png");
        }

        private static IFormFile[] Create3DFiles()
        {
            return new[] { CreateStlFile("model.stl") };
        }

        private static IFormFile[] Create3DAndImageFiles()
        {
            return new[] { CreateStlFile("model.stl"), CreatePngFile("preview.png") };
        }

        private static IFormFile CreateStlFile(string name)
        {
            var stlBytes = new byte[1024];
            ReadOnlySpan<byte> solid = "solid"u8;
            solid.CopyTo(stlBytes);

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(name);
            file.Setup(f => f.Length).Returns(stlBytes.Length);
            file.Setup(f => f.ContentType).Returns("application/octet-stream");
            file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(stlBytes));
            return file.Object;
        }

        private static IFormFile CreatePngFile(string name)
        {
            var pngBytes = new byte[64];
            ReadOnlySpan<byte> pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            pngHeader.CopyTo(pngBytes);

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(name);
            file.Setup(f => f.Length).Returns(pngBytes.Length);
            file.Setup(f => f.ContentType).Returns("image/png");
            file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(pngBytes));
            return file.Object;
        }

        private ClaimsPrincipal CreateUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }
    }
}
