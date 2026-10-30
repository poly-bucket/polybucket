using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;
using ApiUpdateStorageSettings = PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Http;
using PolyBucket.Storage;
using PolyBucket.Storage.UpdateStorageSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateStorageSettings;

public class UpdateStorageSettingsControllerTests
{
    [Fact(DisplayName = "UpdateStorageSettings returns Ok when service succeeds.")]
    public async Task UpdateStorageSettings_ReturnsOk()
    {
        // Arrange
        var service = new Mock<ApiUpdateStorageSettings.IUpdateStorageSettingsService>();
        service.Setup(s => s.UpdateStorageSettingsAsync(It.IsAny<StorageSettingsUpdate>(), It.IsAny<CancellationToken>())).ReturnsAsync(new StorageSettingsDto());
        var controller = new UpdateStorageSettingsController(service.Object);

        // Act
        var result = await controller.UpdateStorageSettings(new UpdateStorageSettingsRequest { Provider = "minio", Endpoint = "localhost", BucketName = "b", AccessKey = "a" });

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
