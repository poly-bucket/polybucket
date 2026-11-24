using Moq;
using PolyBucket.Api.Features.Files.GetFileConfig.Domain;
using PolyBucket.Api.Features.Files.GetFileConfig.Http;
using PolyBucket.Api.Features.Files.GetFileConfig.Repository;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Storage;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.GetFileConfig.Domain;

public class GetFileConfigServiceTests
{
    [Fact(DisplayName = "When site upload limits differ from per-type maxima, file config uses site limits.")]
    public async Task GetFileConfigAsync_UsesSiteLimitsFromProvider()
    {
        // Arrange
        var repository = new Mock<IGetFileConfigRepository>();
        repository
            .Setup(r => r.GetEnabledFileTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FileTypeConfig>
            {
                new()
                {
                    Extension = ".stl",
                    Category = "3D",
                    MaxFileSizeBytes = 500 * 1024 * 1024,
                    MaxPerUpload = 50
                }
            });
        repository
            .Setup(r => r.GetEffectiveStorageSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveStorageSettings { IsConfigured = true, Provider = "local" });

        var limitsProvider = new Mock<IModelUploadLimitsProvider>();
        limitsProvider
            .Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelUploadLimitsSnapshot
            {
                SiteMaxFileSizeBytes = 25 * 1024 * 1024,
                MaxFilesPerUpload = 3
            });

        var service = new GetFileConfigService(repository.Object, limitsProvider.Object);

        // Act
        var result = await service.GetFileConfigAsync(CancellationToken.None);

        // Assert
        result.MaxTotalFileSize.ShouldBe(25 * 1024 * 1024);
        result.MaxFilesPerUpload.ShouldBe(3);
    }
}