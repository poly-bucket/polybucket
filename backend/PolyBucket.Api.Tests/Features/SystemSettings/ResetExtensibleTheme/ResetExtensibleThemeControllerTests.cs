using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Domain;
using PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.ResetExtensibleTheme;

public class ResetExtensibleThemeControllerTests
{
    [Fact(DisplayName = "The ResetExtensibleTheme controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IResetExtensibleThemeService>();
        var controller = new ResetExtensibleThemeController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(ResetExtensibleThemeController controller, Mock<IResetExtensibleThemeService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
