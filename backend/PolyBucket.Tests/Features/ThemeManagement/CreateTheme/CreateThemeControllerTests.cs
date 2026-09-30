using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ThemeManagement.CreateTheme;

public class CreateThemeControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private static ThemeColorsDto Colors() => new()
    {
        Primary = "#000",
        PrimaryLight = "#111",
        PrimaryDark = "#222",
        Secondary = "#333",
        SecondaryLight = "#444",
        SecondaryDark = "#555",
        Accent = "#666",
        AccentLight = "#777",
        AccentDark = "#888",
        BackgroundPrimary = "#fff",
        BackgroundSecondary = "#eee",
        BackgroundTertiary = "#ddd"
    };

    [Fact(DisplayName = "When theme creation fails, create theme returns BadRequest.")]
    public async Task CreateTheme_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new ThemeResponse { Success = false, Message = "duplicate" };
        _mediator.SetupSend<CreateThemeCommand, ThemeResponse>(response);
        var controller = new CreateThemeController(_mediator.Object);
        var request = new CreateThemeRequest { Name = "Dark", Colors = Colors() };

        // Act
        var result = await controller.CreateTheme(request);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When theme creation succeeds, create theme returns Ok.")]
    public async Task CreateTheme_Success_ReturnsOk()
    {
        // Arrange
        var response = new ThemeResponse { Success = true, Message = "created" };
        _mediator.SetupSend<CreateThemeCommand, ThemeResponse>(response);
        var controller = new CreateThemeController(_mediator.Object);
        var request = new CreateThemeRequest { Name = "Dark", Colors = Colors() };

        // Act
        var result = await controller.CreateTheme(request);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
