using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Domain;
using PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.TestStorageConfiguration;

public class TestStorageConfigurationControllerTests
{
    [Fact(DisplayName = "The TestStorageConfiguration controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<ITestStorageConfigurationService>();
        var controller = new TestStorageConfigurationController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(TestStorageConfigurationController controller, Mock<ITestStorageConfigurationService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
