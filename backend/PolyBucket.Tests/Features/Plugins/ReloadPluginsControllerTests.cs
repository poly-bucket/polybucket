using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Commands;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins;

public class ReloadPluginsControllerTests
{
    [Fact(DisplayName = "Reload plugins completes and returns Ok.")]
    public async Task ReloadPlugins_ReturnsOk()
    {
        // Arrange
        var manager = new PluginManager(Path.Combine(Path.GetTempPath(), "polybucket-reload-tests", Guid.NewGuid().ToString("N")));
        var controller = new ReloadPluginsController(manager);

        // Act
        var result = await controller.ReloadPlugins();

        // Assert
        result.ShouldBeOfType<OkResult>();
    }
}
