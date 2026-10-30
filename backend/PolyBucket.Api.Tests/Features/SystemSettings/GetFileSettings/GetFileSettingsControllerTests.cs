using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetFileSettings;

public class GetFileSettingsControllerTests
{
    [Fact(DisplayName = "When file settings load successfully, get file settings returns Ok.")]
    public async Task GetFileSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetFileSettingsResponse { Success = true };
        var service = new Mock<IGetFileSettingsService>();
        service.Setup(s => s.GetFileSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new GetFileSettingsController(service.Object);

        // Act
        var result = await controller.GetFileSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
