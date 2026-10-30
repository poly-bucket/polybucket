using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Domain;
using PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetInstalledPlugins;

public class GetInstalledPluginsControllerTests
{
    [Fact(DisplayName = "When installed plugins are listed, GetInstalledPlugins returns Ok.")]
    public async Task GetInstalledPlugins_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetInstalledPluginsService>();
        service.Setup(s => s.GetInstalledPluginsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InstalledPlugin>());
        var controller = new GetInstalledPluginsController(service.Object);

        // Act
        var result = await controller.GetInstalledPlugins(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeAssignableTo<List<InstalledPlugin>>();
    }
}
