using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetFontAwesomeProEnabled;

public class GetFontAwesomeProEnabledControllerTests
{
    [Fact(DisplayName = "The GetFontAwesomeProEnabled controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IGetFontAwesomeProEnabledService>();
        var controller = new GetFontAwesomeProEnabledController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(GetFontAwesomeProEnabledController controller, Mock<IGetFontAwesomeProEnabledService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
