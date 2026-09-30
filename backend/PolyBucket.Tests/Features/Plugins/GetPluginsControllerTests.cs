using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Queries;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins;

public class GetPluginsControllerTests
{
    [Fact(DisplayName = "When plugins are loaded, get plugins returns Ok with plugin info.")]
    public void GetPlugins_ReturnsOk()
    {
        // Arrange
        var plugin = new Mock<IPlugin>();
        plugin.SetupGet(p => p.Id).Returns("comments");
        plugin.SetupGet(p => p.Name).Returns("Comments");
        plugin.SetupGet(p => p.Version).Returns("1.0.0");
        plugin.SetupGet(p => p.Author).Returns("PolyBucket");
        plugin.SetupGet(p => p.Description).Returns("Comments plugin");
        var manager = PluginManagerTestHelper.WithPlugins(plugin.Object);
        var controller = new GetPluginsController(manager);

        // Act
        var result = controller.GetPlugins();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value.ShouldBeAssignableTo<IEnumerable<PluginInfo>>();
        items.Single().Id.ShouldBe("comments");
    }
}
