using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Queries;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins;

public class GetPluginDetailsControllerTests
{
    [Fact(DisplayName = "When the plugin id is unknown, get plugin details returns NotFound.")]
    public async Task GetPluginDetails_Missing_ReturnsNotFound()
    {
        // Arrange
        var manager = PluginManagerTestHelper.WithPlugins();
        var controller = new GetPluginDetailsController(manager);

        // Act
        var result = await controller.GetPluginDetails("missing");

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the plugin is loaded, get plugin details returns Ok.")]
    public async Task GetPluginDetails_Found_ReturnsOk()
    {
        // Arrange
        var plugin = new Mock<IPlugin>();
        plugin.SetupGet(p => p.Id).Returns("meta");
        plugin.SetupGet(p => p.Name).Returns("Metadata");
        plugin.SetupGet(p => p.Version).Returns("1.0.0");
        plugin.SetupGet(p => p.Author).Returns("PolyBucket");
        plugin.SetupGet(p => p.Description).Returns("Metadata");
        plugin.SetupGet(p => p.Metadata).Returns(new PluginMetadata());
        plugin.SetupGet(p => p.FrontendComponents).Returns([]);
        var manager = PluginManagerTestHelper.WithPlugins(plugin.Object);
        var controller = new GetPluginDetailsController(manager);

        // Act
        var result = await controller.GetPluginDetails("meta");

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "Get plugins overview returns counts for loaded plugins.")]
    public async Task GetPluginsOverview_ReturnsOk()
    {
        // Arrange
        var plugin = new Mock<IPlugin>();
        plugin.SetupGet(p => p.Id).Returns("p1");
        plugin.SetupGet(p => p.Name).Returns("P1");
        plugin.SetupGet(p => p.Version).Returns("1");
        plugin.SetupGet(p => p.Author).Returns("a");
        plugin.SetupGet(p => p.Description).Returns("d");
        plugin.SetupGet(p => p.FrontendComponents).Returns([]);
        plugin.SetupGet(p => p.Metadata).Returns(new PluginMetadata());
        var manager = PluginManagerTestHelper.WithPlugins(plugin.Object);
        var controller = new GetPluginDetailsController(manager);

        // Act
        var result = await controller.GetPluginsOverview();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var body = ok.Value.ShouldBeOfType<PluginOverviewResponse>();
        body.TotalPlugins.ShouldBe(1);
    }
}
