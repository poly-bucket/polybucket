using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Services;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.CreateModel;

public class ModelUploadServiceTests
{
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IStorageObjectKeyResolver> _objectKeyResolver = new();
    private readonly Mock<IModelPreviewGenerationService> _previewGeneration = new();

    private ModelUploadService CreateService() =>
        new(
            _storage.Object,
            _objectKeyResolver.Object,
            _previewGeneration.Object,
            NullLogger<ModelUploadService>.Instance);

    [Fact(DisplayName = "When the model already has a thumbnail, ProcessModelUploadAsync does not request a presigned URL.")]
    public async Task ProcessModelUploadAsync_WithThumbnail_SkipsPresign()
    {
        // Arrange
        var model = new Model
        {
            Id = Guid.NewGuid(),
            ThumbnailUrl = "thumbnails/existing.jpg",
            Files = []
        };
        var service = CreateService();

        // Act
        await service.ProcessModelUploadAsync(model);

        // Assert
        _storage.Verify(
            s => s.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "When there is no 3D model file, ProcessModelUploadAsync does not request a presigned URL.")]
    public async Task ProcessModelUploadAsync_No3DFile_SkipsPresign()
    {
        // Arrange
        var model = new Model
        {
            Id = Guid.NewGuid(),
            ThumbnailUrl = null,
            Files =
            [
                new ModelFile
                {
                    Id = Guid.NewGuid(),
                    Name = "readme.pdf",
                    Path = "models/readme.pdf"
                }
            ]
        };
        var service = CreateService();

        // Act
        await service.ProcessModelUploadAsync(model);

        // Assert
        _storage.Verify(
            s => s.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "When a supported 3D file exists, ProcessModelUploadAsync presigns and sets the thumbnail on success.")]
    public async Task ProcessModelUploadAsync_Supported3DFile_SetsThumbnail()
    {
        // Arrange
        const string objectKey = "models/abc/model.stl";
        var model = new Model
        {
            Id = Guid.NewGuid(),
            ThumbnailUrl = null,
            Files =
            [
                new ModelFile
                {
                    Id = Guid.NewGuid(),
                    Name = "model.stl",
                    Path = objectKey
                }
            ]
        };
        _objectKeyResolver.Setup(r => r.Resolve(objectKey)).Returns(objectKey);
        _previewGeneration.Setup(p => p.IsSupportedFileTypeAsync(".stl")).ReturnsAsync(true);
        _storage
            .Setup(s => s.GetPresignedUrlAsync(objectKey, StorageAccessDurations.ThumbnailSourceUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost:8333/signed");
        _previewGeneration
            .Setup(p => p.GeneratePreviewAsync(
                model.Id,
                "http://localhost:8333/signed",
                ".stl",
                "thumbnail",
                It.IsAny<PreviewGenerationSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelPreview
            {
                Status = PreviewStatus.Completed,
                PreviewUrl = "previews/thumb.jpg"
            });
        var service = CreateService();

        // Act
        await service.ProcessModelUploadAsync(model);

        // Assert
        model.ThumbnailUrl.ShouldBe("previews/thumb.jpg");
    }

    [Fact(DisplayName = "When the 3D file type is unsupported, ProcessModelUploadAsync does not request a presigned URL.")]
    public async Task ProcessModelUploadAsync_UnsupportedExtension_SkipsPresign()
    {
        // Arrange
        var model = new Model
        {
            Id = Guid.NewGuid(),
            ThumbnailUrl = null,
            Files =
            [
                new ModelFile
                {
                    Id = Guid.NewGuid(),
                    Name = "model.stl",
                    Path = "models/model.stl"
                }
            ]
        };
        _objectKeyResolver.Setup(r => r.Resolve("models/model.stl")).Returns("models/model.stl");
        _previewGeneration.Setup(p => p.IsSupportedFileTypeAsync(".stl")).ReturnsAsync(false);
        var service = CreateService();

        // Act
        await service.ProcessModelUploadAsync(model);

        // Assert
        _storage.Verify(
            s => s.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
