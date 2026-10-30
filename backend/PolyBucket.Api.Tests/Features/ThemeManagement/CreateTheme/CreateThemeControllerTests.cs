using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme.Domain;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ThemeManagement.CreateTheme;

public class CreateThemeControllerTests
{
    private readonly Mock<ICreateThemeService> _service = new();
    private readonly CreateThemeController _controller;

    public CreateThemeControllerTests()
    {
        _controller = new CreateThemeController(_service.Object);
    }

    [Fact(DisplayName = "When theme creation succeeds, CreateTheme returns Ok.")]
    public async Task CreateTheme_Success_ReturnsOk()
    {
        _service.Setup(s => s.CreateThemeAsync(It.IsAny<CreateThemeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateThemeResponse { Success = true, Message = "ok" });
        var result = await _controller.CreateTheme(new CreateThemeRequest
        {
            Name = "t",
            Colors = new ThemeColorsDto { Primary = "#000", PrimaryLight = "#111", PrimaryDark = "#222", Secondary = "#333", SecondaryLight = "#444", SecondaryDark = "#555", Accent = "#666", AccentLight = "#777", AccentDark = "#888", BackgroundPrimary = "#999", BackgroundSecondary = "#aaa", BackgroundTertiary = "#bbb" }
        }, CancellationToken.None);
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
