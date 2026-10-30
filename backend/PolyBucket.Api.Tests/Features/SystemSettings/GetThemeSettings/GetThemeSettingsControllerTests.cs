using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetThemeSettings;

public class GetThemeSettingsControllerTests
{
    [Fact(DisplayName = "When theme settings load successfully, GetThemeSettings returns Ok.")]
    public async Task GetThemeSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetThemeSettingsResponse { Success = true };
        var service = new Mock<IGetThemeSettingsService>();
        service.Setup(s => s.GetThemeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new GetThemeSettingsController(service.Object);

        // Act
        var result = await controller.GetThemeSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When theme settings fail to load, GetThemeSettings returns BadRequest.")]
    public async Task GetThemeSettings_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new GetThemeSettingsResponse { Success = false };
        var service = new Mock<IGetThemeSettingsService>();
        service.Setup(s => s.GetThemeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new GetThemeSettingsController(service.Object);

        // Act
        var result = await controller.GetThemeSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }
}
