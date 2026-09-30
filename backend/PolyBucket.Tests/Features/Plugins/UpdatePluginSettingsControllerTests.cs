using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Commands;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins;

public class UpdatePluginSettingsControllerTests
{
    [Fact(DisplayName = "When the plugin id is unknown, get plugin settings returns NotFound.")]
    public async Task GetPluginSettings_Missing_ReturnsNotFound()
    {
        // Arrange
        var manager = PluginManagerTestHelper.WithPlugins();
        var controller = new UpdatePluginSettingsController(manager);

        // Act
        var result = await controller.GetPluginSettings("missing");

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When disabling a plugin that cannot be disabled, update status returns BadRequest.")]
    public async Task UpdatePluginStatus_CannotDisable_ReturnsBadRequest()
    {
        // Arrange
        var plugin = new Mock<IPlugin>();
        plugin.SetupGet(p => p.Id).Returns("core");
        plugin.SetupGet(p => p.Metadata).Returns(new PluginMetadata { Lifecycle = new PluginLifecycle { CanDisable = false } });
        var manager = PluginManagerTestHelper.WithPlugins(plugin.Object);
        var controller = new UpdatePluginSettingsController(manager);

        // Act
        var result = await controller.UpdatePluginStatus("core", new UpdatePluginStatusRequest { Enabled = false });

        // Assert
        var bad = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        bad.Value.ShouldBeOfType<PluginStatusResponse>().Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "When settings include an unknown key, update plugin settings returns BadRequest.")]
    public async Task UpdatePluginSettings_InvalidKey_ReturnsBadRequest()
    {
        // Arrange
        var plugin = new Mock<IPlugin>();
        plugin.SetupGet(p => p.Id).Returns("cfg");
        plugin.SetupGet(p => p.Metadata).Returns(new PluginMetadata
        {
            Settings = new Dictionary<string, PluginSetting>
            {
                ["enabled"] = new PluginSetting { Required = true, Type = PluginSettingType.Boolean }
            }
        });
        var manager = PluginManagerTestHelper.WithPlugins(plugin.Object);
        var controller = new UpdatePluginSettingsController(manager);

        // Act
        var result = await controller.UpdatePluginSettings("cfg", new UpdatePluginSettingsRequest
        {
            Settings = new Dictionary<string, object> { ["unknown"] = true }
        });

        // Assert
        var bad = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        bad.Value.ShouldBeOfType<UpdatePluginSettingsResponse>().Success.ShouldBeFalse();
    }
}
