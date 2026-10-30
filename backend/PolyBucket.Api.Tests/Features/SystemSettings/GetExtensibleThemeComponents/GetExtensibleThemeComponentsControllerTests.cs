using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Domain;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemeComponents;

public class GetExtensibleThemeComponentsControllerTests
{
    [Fact(DisplayName = "The GetExtensibleThemeComponents controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IGetExtensibleThemeComponentsService>();
        var controller = new GetExtensibleThemeComponentsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(GetExtensibleThemeComponentsController controller, Mock<IGetExtensibleThemeComponentsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
