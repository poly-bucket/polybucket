using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateExtensibleThemeConfiguration;

public class UpdateExtensibleThemeConfigurationControllerTests
{
    [Fact(DisplayName = "When configuration is null, UpdateConfiguration returns BadRequest.")]
    public async Task UpdateConfiguration_Null_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IUpdateExtensibleThemeConfigurationService>();
        var controller = new UpdateExtensibleThemeConfigurationController(service.Object).WithUser(Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.UpdateConfiguration(null!, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
