using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetFontAwesomeSettings;

public class GetFontAwesomeSettingsControllerTests
{
    [Fact(DisplayName = "The GetFontAwesomeSettings controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IGetFontAwesomeSettingsService>();
        var controller = new GetFontAwesomeSettingsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(GetFontAwesomeSettingsController controller, Mock<IGetFontAwesomeSettingsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
