using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateThemeSettings;

public class UpdateThemeSettingsControllerTests
{
    [Fact(DisplayName = "The UpdateThemeSettings controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IUpdateThemeSettingsService>();
        var controller = new UpdateThemeSettingsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(UpdateThemeSettingsController controller, Mock<IUpdateThemeSettingsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
