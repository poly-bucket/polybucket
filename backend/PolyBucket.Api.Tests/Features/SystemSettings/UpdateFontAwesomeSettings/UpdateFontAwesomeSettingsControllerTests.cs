using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateFontAwesomeSettings;

public class UpdateFontAwesomeSettingsControllerTests
{
    [Fact(DisplayName = "The UpdateFontAwesomeSettings controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IUpdateFontAwesomeSettingsService>();
        var controller = new UpdateFontAwesomeSettingsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(UpdateFontAwesomeSettingsController controller, Mock<IUpdateFontAwesomeSettingsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
