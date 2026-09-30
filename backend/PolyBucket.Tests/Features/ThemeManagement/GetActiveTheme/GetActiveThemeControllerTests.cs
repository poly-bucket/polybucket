using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ThemeManagement.GetActiveTheme;

public class GetActiveThemeControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When no active theme exists, get active theme returns NotFound.")]
    public async Task GetActiveTheme_None_ReturnsNotFound()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetActiveThemeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((ThemeDto?)null);
        var controller = new GetActiveThemeController(_mediator.Object);

        // Act
        var result = await controller.GetActiveTheme();

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When an active theme exists, get active theme returns Ok.")]
    public async Task GetActiveTheme_Found_ReturnsOk()
    {
        // Arrange
        var theme = new ThemeDto { Id = 1, Name = "Default", Colors = new ThemeColorsDto { Primary = "#000", PrimaryLight = "#111", PrimaryDark = "#222", Secondary = "#333", SecondaryLight = "#444", SecondaryDark = "#555", Accent = "#666", AccentLight = "#777", AccentDark = "#888", BackgroundPrimary = "#fff", BackgroundSecondary = "#eee", BackgroundTertiary = "#ddd" } };
        _mediator.Setup(m => m.Send(It.IsAny<GetActiveThemeQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(theme);
        var controller = new GetActiveThemeController(_mediator.Object);

        // Act
        var result = await controller.GetActiveTheme();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(theme);
    }
}
