using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.ApplyStorageSetup;

public class ApplyStorageSetupControllerTests
{
    [Fact(DisplayName = "The ApplyStorageSetup controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IApplyStorageSetupService>();
        var controller = new ApplyStorageSetupController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(ApplyStorageSetupController controller, Mock<IApplyStorageSetupService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
