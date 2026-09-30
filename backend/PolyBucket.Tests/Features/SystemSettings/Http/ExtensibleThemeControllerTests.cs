using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class ExtensibleThemeControllerTests
{
    private readonly Mock<IThemeManager> _themeManager = new();

    [Fact(DisplayName = "When themes exist, GetAvailableThemes returns Ok.")]
    public async Task GetAvailableThemes_ReturnsOk()
    {
        // Arrange
        _themeManager.Setup(m => m.GetAllThemesAsync()).ReturnsAsync(new List<ThemeDefinition>());
        var controller = new ExtensibleThemeController(_themeManager.Object);

        // Act
        var result = await controller.GetAvailableThemes();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When no active theme is set, GetActiveTheme returns NotFound.")]
    public async Task GetActiveTheme_Missing_ReturnsNotFound()
    {
        // Arrange
        _themeManager.Setup(m => m.GetActiveThemeAsync()).ReturnsAsync((ThemeDefinition?)null);
        var controller = new ExtensibleThemeController(_themeManager.Object);

        // Act
        var result = await controller.GetActiveTheme();

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When set active theme fails, SetActiveTheme returns BadRequest.")]
    public async Task SetActiveTheme_Failure_ReturnsBadRequest()
    {
        // Arrange
        _themeManager.Setup(m => m.SetActiveThemeAsync("dark")).ReturnsAsync(false);
        var controller = new ExtensibleThemeController(_themeManager.Object).WithUser(Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.SetActiveTheme("dark");

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When configuration is null, UpdateConfiguration returns BadRequest.")]
    public async Task UpdateConfiguration_Null_ReturnsBadRequest()
    {
        // Arrange
        var controller = new ExtensibleThemeController(_themeManager.Object).WithUser(Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.UpdateConfiguration(null!);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
