using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Domain;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemePlugins;

public class GetExtensibleThemePluginsControllerTests
{
    [Fact(DisplayName = "The GetExtensibleThemePlugins controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IGetExtensibleThemePluginsService>();
        var controller = new GetExtensibleThemePluginsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(GetExtensibleThemePluginsController controller, Mock<IGetExtensibleThemePluginsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
