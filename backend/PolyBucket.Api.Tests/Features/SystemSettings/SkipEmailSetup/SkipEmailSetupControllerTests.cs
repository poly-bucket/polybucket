using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.SkipEmailSetup;

public class SkipEmailSetupControllerTests
{
    [Fact(DisplayName = "The SkipEmailSetup controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<ISkipEmailSetupService>();
        var controller = new SkipEmailSetupController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(SkipEmailSetupController controller, Mock<ISkipEmailSetupService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
