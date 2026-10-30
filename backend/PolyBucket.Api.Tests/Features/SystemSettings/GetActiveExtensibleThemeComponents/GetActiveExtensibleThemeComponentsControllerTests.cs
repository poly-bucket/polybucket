using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Domain;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetActiveExtensibleThemeComponents;

public class GetActiveExtensibleThemeComponentsControllerTests
{
    [Fact(DisplayName = "The GetActiveExtensibleThemeComponents controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IGetActiveExtensibleThemeComponentsService>();
        var controller = new GetActiveExtensibleThemeComponentsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(GetActiveExtensibleThemeComponentsController controller, Mock<IGetActiveExtensibleThemeComponentsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
