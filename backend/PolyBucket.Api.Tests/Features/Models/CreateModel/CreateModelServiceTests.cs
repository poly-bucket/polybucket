using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using PolyBucket.Api.Features.Models.CreateModel.Repository;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Api.Features.Models.Upload;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.CreateModel
{
    public class CreateModelServiceTests
    {
        private readonly Mock<ICreateModelRepository> _mockRepository;
        private readonly Mock<IStorageService> _mockStorage;
        private readonly Mock<IModelModerationEnqueueService> _mockModerationEnqueueService;
        private readonly Mock<IModelPreviewQueue> _mockPreviewQueue;
        private readonly Mock<IModelUploadValidator> _mockUploadValidator;
        private readonly Mock<IModelMetadataValidator> _mockMetadataValidator;
        private readonly Mock<IModelMetadataPolicy> _mockMetadataPolicy;
        private readonly Mock<ISiteModelUploadPolicy> _mockUploadPolicy;
        private readonly Mock<ISiteModelFeaturePolicy> _mockFeaturePolicy;
        private readonly Mock<IAnonymousUploadAuthorProvider> _mockAnonymousAuthor;
        private readonly Mock<ISiteUploadCompressionPolicy> _mockCompressionPolicy;
        private readonly Mock<ILogger<CreateModelService>> _mockLogger;
        private readonly CreateModelService _service;

        public CreateModelServiceTests()
        {
            _mockRepository = new Mock<ICreateModelRepository>();
            _mockStorage = new Mock<IStorageService>();
            _mockModerationEnqueueService = new Mock<IModelModerationEnqueueService>();
            _mockPreviewQueue = new Mock<IModelPreviewQueue>();
            _mockUploadValidator = new Mock<IModelUploadValidator>();
            _mockUploadPolicy = new Mock<ISiteModelUploadPolicy>();
            _mockFeaturePolicy = new Mock<ISiteModelFeaturePolicy>();
            _mockFeaturePolicy
                .Setup(p => p.ValidateModelCreationQuotaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModelCreationQuotaOutcome.Allowed);
            _mockAnonymousAuthor = new Mock<IAnonymousUploadAuthorProvider>();
            _mockCompressionPolicy = new Mock<ISiteUploadCompressionPolicy>();
            _mockCompressionPolicy
                .Setup(p => p.ShouldCompressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockLogger = new Mock<ILogger<CreateModelService>>();
            _mockUploadValidator
                .Setup(v => v.ValidateAsync(It.IsAny<IFormFile[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
            _mockMetadataValidator = new Mock<IModelMetadataValidator>();
            _mockMetadataPolicy = new Mock<IModelMetadataPolicy>();
            _mockMetadataPolicy
                .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModelMetadataPolicySnapshot.FailClosed with { RequireModelDescription = false, RequireLicenseSelection = false, RequireThumbnail = false });
            _mockMetadataValidator
                .Setup(v => v.ValidateCreateMetadata(
                    It.IsAny<ModelMetadataPolicySnapshot>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<LicenseTypes?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<string?>(),
                    It.IsAny<IFormFile[]>()))
                .Returns((string?)null);
            _mockUploadPolicy
                .Setup(p => p.GetDefaultModelPrivacyAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(PrivacySettings.Public);
            _mockUploadPolicy
                .Setup(p => p.IsAnonymousModelUploadAllowedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _service = new CreateModelService(
                _mockRepository.Object,
                _mockStorage.Object,
                _mockModerationEnqueueService.Object,
                _mockPreviewQueue.Object,
                _mockUploadValidator.Object,
                _mockMetadataValidator.Object,
                _mockMetadataPolicy.Object,
                _mockUploadPolicy.Object,
                _mockFeaturePolicy.Object,
                _mockAnonymousAuthor.Object,
                _mockCompressionPolicy.Object,
                _mockLogger.Object);
        }

        [Fact(DisplayName = "When a model is created, a background preview is queued for it.")]
        public async Task CreateModelAsync_WithValidRequest_QueuesPreview()
        {
            // Arrange
            var request = new CreateModelRequest { Name = "Preview Model", Privacy = "public", Files = CreateTestFiles() };
            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://storage.example.com/test-file.stl");
            _mockRepository.Setup(x => x.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Model model, CancellationToken ct) => model);

            // Act
            var result = await _service.CreateModelAsync(request, CreateTestUser(), CancellationToken.None);

            // Assert
            _mockPreviewQueue.Verify(q => q.EnqueueForNewContentAsync(result.Model.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When creating a model with a valid request, the create model service creates the model.")]
        public async Task CreateModelAsync_WithValidRequest_ShouldCreateModel()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Description = "Test Description",
                Privacy = "public",
                License = "mit",
                AIGenerated = false,
                WorkInProgress = false,
                NSFW = false,
                Remix = false,
                Files = CreateTestFiles()
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("https://storage.example.com/test-file.stl");

            _mockRepository.Setup(x => x.CreateModelAsync(It.IsAny<Model>(), cancellationToken))
                .ReturnsAsync((Model model, CancellationToken ct) => model);

            // Act
            var result = await _service.CreateModelAsync(request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Model.ShouldNotBeNull();
            result.Model.Name.ShouldBe("Test Model");
            result.Model.Description.ShouldBe("Test Description");
            
            _mockRepository.Verify(x => x.CreateModelAsync(It.IsAny<Model>(), cancellationToken), Times.Once);
            _mockStorage.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken), Times.AtLeastOnce);
        }

        [Fact(DisplayName = "When creating a model with only 3D files, the create model service leaves the thumbnail unset.")]
        public async Task CreateModelAsync_WithOnly3DFiles_ShouldLeaveThumbnailNull()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Privacy = "public",
                Files = CreateTestFiles()
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("https://storage.example.com/test-file.stl");

            _mockRepository.Setup(x => x.CreateModelAsync(It.IsAny<Model>(), cancellationToken))
                .ReturnsAsync((Model model, CancellationToken ct) => model);

            // Act
            var result = await _service.CreateModelAsync(request, user, cancellationToken);

            // Assert
            result.Model.ShouldNotBeNull();
            result.Model.ThumbnailUrl.ShouldBeNull();
        }

        [Fact(DisplayName = "When creating a model that includes an image file, the create model service uses the image as the thumbnail.")]
        public async Task CreateModelAsync_WithImageFile_ShouldSetThumbnailToImage()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Privacy = "public",
                Files = CreateModelAndImageFiles()
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("https://storage.example.com/test-file");

            _mockRepository.Setup(x => x.CreateModelAsync(It.IsAny<Model>(), cancellationToken))
                .ReturnsAsync((Model model, CancellationToken ct) => model);

            // Act
            var result = await _service.CreateModelAsync(request, user, cancellationToken);

            // Assert
            result.Model.ShouldNotBeNull();
            result.Model.ThumbnailUrl.ShouldNotBeNull();
            result.Model.ThumbnailUrl!.ShouldEndWith("preview.png");
        }

        [Fact(DisplayName = "When upload validation fails, the create model service throws a ValidationException.")]
        public async Task CreateModelAsync_WhenUploadValidationFails_ShouldThrowValidationException()
        {
            // Arrange
            _mockUploadValidator
                .Setup(v => v.ValidateAsync(It.IsAny<IFormFile[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Too many files.");

            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Files = CreateTestFiles()
            };

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, CreateTestUser(), CancellationToken.None));
        }

        [Fact(DisplayName = "When creating a model with an empty name, the create model service throws a ValidationException.")]
        public async Task CreateModelAsync_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "",
                Files = CreateTestFiles()
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, user, cancellationToken));
        }

        [Fact(DisplayName = "When creating a model without files, the create model service throws a ValidationException.")]
        public async Task CreateModelAsync_WithoutFiles_ShouldThrowValidationException()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Files = new IFormFile[0]
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, user, cancellationToken));
        }

        [Fact(DisplayName = "When the caller is unauthenticated and anonymous uploads are off, the create model service throws a ValidationException.")]
        public async Task CreateModelAsync_WithInvalidUser_ShouldThrowValidationException()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Files = CreateTestFiles()
            };

            var user = CreateInvalidUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, user, cancellationToken));
        }

        [Fact(DisplayName = "When privacy is omitted, the create model service uses the site default privacy.")]
        public async Task CreateModelAsync_WhenPrivacyOmitted_UsesSiteDefault()
        {
            // Arrange
            _mockUploadPolicy
                .Setup(p => p.GetDefaultModelPrivacyAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(PrivacySettings.Unlisted);

            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Privacy = null,
                Files = CreateTestFiles()
            };

            Model? captured = null;
            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://storage.example.com/test-file.stl");
            _mockRepository.Setup(x => x.CreateModelAsync(It.IsAny<Model>(), It.IsAny<CancellationToken>()))
                .Callback<Model, CancellationToken>((m, _) => captured = m)
                .ReturnsAsync((Model model, CancellationToken _) => model);

            // Act
            await _service.CreateModelAsync(request, CreateTestUser(), CancellationToken.None);

            // Assert
            captured.ShouldNotBeNull();
            captured!.Privacy.ShouldBe(PrivacySettings.Unlisted);
        }

        private static IFormFile[] CreateTestFiles()
        {
            var stlBytes = new byte[1024];
            ReadOnlySpan<byte> solid = "solid"u8;
            solid.CopyTo(stlBytes);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.stl");
            mockFile.Setup(f => f.Length).Returns(stlBytes.Length);
            mockFile.Setup(f => f.ContentType).Returns("application/octet-stream");
            mockFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(stlBytes));

            return new[] { mockFile.Object };
        }

        private static IFormFile[] CreateModelAndImageFiles()
        {
            var stlBytes = new byte[1024];
            ReadOnlySpan<byte> solid = "solid"u8;
            solid.CopyTo(stlBytes);

            var stlFile = new Mock<IFormFile>();
            stlFile.Setup(f => f.FileName).Returns("model.stl");
            stlFile.Setup(f => f.Length).Returns(stlBytes.Length);
            stlFile.Setup(f => f.ContentType).Returns("application/octet-stream");
            stlFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(stlBytes));

            var pngBytes = new byte[64];
            ReadOnlySpan<byte> pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            pngHeader.CopyTo(pngBytes);

            var imageFile = new Mock<IFormFile>();
            imageFile.Setup(f => f.FileName).Returns("preview.png");
            imageFile.Setup(f => f.Length).Returns(pngBytes.Length);
            imageFile.Setup(f => f.ContentType).Returns("image/png");
            imageFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(pngBytes));

            return new[] { stlFile.Object, imageFile.Object };
        }

        private static ClaimsPrincipal CreateTestUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@example.com")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static ClaimsPrincipal CreateInvalidUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com")
                // Missing NameIdentifier claim
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }
    }
} 