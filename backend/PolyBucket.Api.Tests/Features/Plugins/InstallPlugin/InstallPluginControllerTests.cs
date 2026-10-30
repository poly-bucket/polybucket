using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.InstallPlugin.Domain;
using PolyBucket.Api.Features.Plugins.InstallPlugin.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.InstallPlugin;

public class InstallPluginControllerTests
{
    [Fact(DisplayName = "When installation source is missing, InstallPlugin returns BadRequest.")]
    public async Task InstallPlugin_MissingSource_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IInstallPluginService>();
        service.Setup(s => s.InstallAsync(It.IsAny<PluginInstallationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InstallPluginOutcome { HttpStatus = InstallPluginHttpStatus.BadRequest, ErrorBody = new { message = "Installation source is required" } });
        var controller = new InstallPluginController(service.Object);

        // Act
        var result = await controller.InstallPlugin(new PluginInstallationRequest { Source = " " }, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
