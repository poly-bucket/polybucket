using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.DownloadModel.Domain;
using PolyBucket.Api.Features.Models.DownloadModel.Repository;
using PolyBucket.Api.Features.Models.RecordModelDownload.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DownloadModel;

public class DownloadModelServiceTests
{
    private readonly Mock<IDownloadModelRepository> _repository = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IStorageObjectKeyResolver> _objectKeyResolver = new();
    private readonly Mock<IModelDownloadCounter> _downloadCounter = new();
    private readonly DefaultHttpContext _httpContext = new();

    private DownloadModelService CreateService()
    {
        var accessor = new HttpContextAccessor { HttpContext = _httpContext };
        return new DownloadModelService(
            _repository.Object,
            _permissionService.Object,
            _storage.Object,
            _objectKeyResolver.Object,
            accessor,
            _downloadCounter.Object,
            NullLogger<DownloadModelService>.Instance);
    }

    [Fact(DisplayName = "When the model does not exist, DownloadAsync returns NotFound.")]
    public async Task DownloadAsync_ModelNotFound_ReturnsNotFound()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetBundleForDownloadAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DownloadModelBundle?)null);
        var service = CreateService();

        // Act
        var outcome = await service.DownloadAsync(modelId, new ClaimsPrincipal());

        // Assert
        outcome.Kind.ShouldBe(DownloadModelOutcomeKind.NotFound);
    }

    [Fact(DisplayName = "When the user cannot access a private model, DownloadAsync returns Forbid.")]
    public async Task DownloadAsync_PrivateModel_ReturnsForbid()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var bundle = new DownloadModelBundle
        {
            Id = modelId,
            Name = "Private",
            Privacy = PrivacySettings.Private,
            AuthorId = Guid.NewGuid(),
            Files =
            [
                new DownloadModelFileItem
                {
                    Id = Guid.NewGuid(),
                    Name = "a.stl",
                    Path = "models/a.stl",
                    MimeType = "application/octet-stream"
                }
            ]
        };
        _repository
            .Setup(r => r.GetBundleForDownloadAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        var service = CreateService();

        // Act
        var outcome = await service.DownloadAsync(modelId, new ClaimsPrincipal());

        // Assert
        outcome.Kind.ShouldBe(DownloadModelOutcomeKind.Forbid);
    }

    [Fact(DisplayName = "When a public model has one file, DownloadAsync streams the file from storage.")]
    public async Task DownloadAsync_SinglePublicFile_ReturnsStream()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        const string objectKey = "models/one/file.stl";
        var bundle = new DownloadModelBundle
        {
            Id = modelId,
            Name = "One File",
            Privacy = PrivacySettings.Public,
            AuthorId = Guid.NewGuid(),
            Files =
            [
                new DownloadModelFileItem
                {
                    Id = Guid.NewGuid(),
                    Name = "file.stl",
                    Path = objectKey,
                    MimeType = "application/octet-stream"
                }
            ]
        };
        _repository
            .Setup(r => r.GetBundleForDownloadAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        _objectKeyResolver.Setup(r => r.Resolve(objectKey)).Returns(objectKey);
        var payload = new MemoryStream(Encoding.UTF8.GetBytes("solid test endsolid"));
        _storage
            .Setup(s => s.DownloadAsync(objectKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);
        _downloadCounter
            .Setup(c => c.TryRecordDownloadAsync(modelId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((1, true));
        var service = CreateService();

        // Act
        var outcome = await service.DownloadAsync(modelId, new ClaimsPrincipal());

        // Assert
        outcome.Kind.ShouldBe(DownloadModelOutcomeKind.OkSingleFile);
        outcome.FileName.ShouldBe("file.stl");
        outcome.FileStream.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When the object key cannot be resolved for a single file, DownloadAsync returns Error.")]
    public async Task DownloadAsync_UnresolvableKey_ReturnsError()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var bundle = new DownloadModelBundle
        {
            Id = modelId,
            Name = "Bad Path",
            Privacy = PrivacySettings.Public,
            AuthorId = Guid.NewGuid(),
            Files =
            [
                new DownloadModelFileItem
                {
                    Id = Guid.NewGuid(),
                    Name = "file.stl",
                    Path = "http://invalid/url",
                    MimeType = "application/octet-stream"
                }
            ]
        };
        _repository
            .Setup(r => r.GetBundleForDownloadAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        _objectKeyResolver.Setup(r => r.Resolve(bundle.Files[0].Path)).Returns((string?)null);
        var service = CreateService();

        // Act
        var outcome = await service.DownloadAsync(modelId, new ClaimsPrincipal());

        // Assert
        outcome.Kind.ShouldBe(DownloadModelOutcomeKind.Error);
    }
}
