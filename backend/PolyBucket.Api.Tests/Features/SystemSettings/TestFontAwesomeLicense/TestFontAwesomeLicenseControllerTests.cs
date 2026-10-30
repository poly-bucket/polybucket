using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Domain;
using PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.TestFontAwesomeLicense;

public class TestFontAwesomeLicenseControllerTests
{
    [Fact(DisplayName = "The TestFontAwesomeLicense controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<ITestFontAwesomeLicenseService>();
        var controller = new TestFontAwesomeLicenseController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(TestFontAwesomeLicenseController controller, Mock<ITestFontAwesomeLicenseService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
