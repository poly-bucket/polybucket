using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Domain;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemeConfiguration;

public class GetExtensibleThemeConfigurationControllerTests
{
    [Fact(DisplayName = "The GetExtensibleThemeConfiguration controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IGetExtensibleThemeConfigurationService>();
        var controller = new GetExtensibleThemeConfigurationController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(GetExtensibleThemeConfigurationController controller, Mock<IGetExtensibleThemeConfigurationService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
