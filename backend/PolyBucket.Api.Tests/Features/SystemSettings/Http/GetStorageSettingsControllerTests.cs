using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;
using PolyBucket.Storage;
using PolyBucket.Storage.GetStorageSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Http;

public class GetStorageSettingsControllerTests
{
    [Fact(DisplayName = "When storage settings exist, get storage settings returns Ok.")]
    public async Task GetStorageSettings_ReturnsOk()
    {
        // Arrange
        var dto = new StorageSettingsDto { Provider = StorageProviderIds.Silo, IsConfigured = true };
        var service = new Mock<IGetStorageSettingsService>();
        service.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var controller = new GetStorageSettingsController(service.Object);

        // Act
        var result = await controller.GetStorageSettings();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(dto);
    }
}
