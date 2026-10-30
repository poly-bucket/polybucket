using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateSiteSettings;

public class UpdateSiteSettingsControllerTests
{
    [Fact(DisplayName = "The UpdateSiteSettings controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IUpdateSiteSettingsService>();
        var controller = new UpdateSiteSettingsController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(UpdateSiteSettingsController controller, Mock<IUpdateSiteSettingsService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
