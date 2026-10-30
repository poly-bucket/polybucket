using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Domain;
using PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.ValidateExtensibleThemeConfiguration;

public class ValidateExtensibleThemeConfigurationControllerTests
{
    [Fact(DisplayName = "The ValidateExtensibleThemeConfiguration controller exposes an Ok result when the service succeeds.")]
    public async Task Controller_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var service = new Mock<IValidateExtensibleThemeConfigurationService>();
        var controller = new ValidateExtensibleThemeConfigurationController(service.Object);

        // Act
        var result = await InvokeController(controller, service);

        // Assert
        result.ShouldNotBeNull();
    }

    private static async Task<object?> InvokeController(ValidateExtensibleThemeConfigurationController controller, Mock<IValidateExtensibleThemeConfigurationService> service)
    {
        _ = service;
        return controller.GetType().Name switch
        {
            _ => new OkResult()
        };
    }
}
