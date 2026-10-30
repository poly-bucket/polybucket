using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.UninstallPlugin.Domain;
using PolyBucket.Api.Features.Plugins.UninstallPlugin.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.UninstallPlugin;

public class UninstallPluginControllerTests
{
    [Fact(DisplayName = "When plugin id is empty, UninstallPlugin returns BadRequest.")]
    public async Task UninstallPlugin_EmptyId_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IUninstallPluginService>();
        service.Setup(s => s.UninstallAsync(" ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UninstallPluginOutcome { Status = UninstallPluginStatus.BadRequest, PluginId = " " });
        var controller = new UninstallPluginController(service.Object);

        // Act
        var result = await controller.UninstallPlugin(" ", CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
