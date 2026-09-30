using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins.Http;

public class ThemePluginControllerTests
{
    [Fact(DisplayName = "When no themes are active, GetActiveThemes returns Ok with an empty list.")]
    public void GetActiveThemes_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetActiveThemes();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeAssignableTo<System.Collections.Generic.List<ActiveTheme>>();
    }

    [Fact(DisplayName = "When plugin id is empty, ApplyTheme returns BadRequest.")]
    public async Task ApplyTheme_EmptyPluginId_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.ApplyTheme(" ");

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When plugin id is empty, RemoveTheme returns BadRequest.")]
    public async Task RemoveTheme_EmptyPluginId_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.RemoveTheme(" ");

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When theme is not active, RemoveTheme returns BadRequest.")]
    public async Task RemoveTheme_NotActive_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.RemoveTheme("dark-theme");

        // Assert
        var bad = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        bad.Value.ShouldBeOfType<ThemeApplicationResult>().Success.ShouldBeFalse();
    }

    private static ThemePluginController CreateController() =>
        new(new ThemePluginService(NullLogger<ThemePluginService>.Instance), NullLogger<ThemePluginController>.Instance);
}
