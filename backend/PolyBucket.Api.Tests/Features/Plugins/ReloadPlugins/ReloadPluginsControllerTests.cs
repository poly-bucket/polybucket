using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.ReloadPlugins.Domain;
using PolyBucket.Api.Features.Plugins.ReloadPlugins.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.ReloadPlugins;

public class ReloadPluginsControllerTests
{
    [Fact(DisplayName = "Reload plugins completes and returns Ok.")]
    public async Task ReloadPlugins_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IReloadPluginsService>();
        service.Setup(s => s.ReloadAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new ReloadPluginsController(service.Object);

        // Act
        var result = await controller.ReloadPlugins(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkResult>();
    }
}
