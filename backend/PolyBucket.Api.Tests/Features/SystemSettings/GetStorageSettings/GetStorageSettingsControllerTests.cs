using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;
using PolyBucket.Storage;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetStorageSettings;

public class GetStorageSettingsControllerTests
{
    [Fact(DisplayName = "GetStorageSettings returns Ok.")]
    public async Task GetStorageSettings_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetStorageSettingsService>();
        service.Setup(s => s.GetStorageSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new StorageSettingsDto());
        var controller = new GetStorageSettingsController(service.Object);

        // Act
        var result = await controller.GetStorageSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
