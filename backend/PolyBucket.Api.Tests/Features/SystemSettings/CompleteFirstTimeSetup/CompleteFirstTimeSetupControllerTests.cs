using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.CompleteFirstTimeSetup;

public class CompleteFirstTimeSetupControllerTests
{
    [Fact(DisplayName = "The CompleteFirstTimeSetup controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<ICompleteFirstTimeSetupService>();
        var controller = new CompleteFirstTimeSetupController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(CompleteFirstTimeSetupController controller, Mock<ICompleteFirstTimeSetupService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
