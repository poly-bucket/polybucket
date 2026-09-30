using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Api.Features.SystemSettings.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class TokenSettingsControllerTests
{
    private readonly Mock<ITokenSettingsService> _service = new();
    private readonly TokenSettingsController _controller;

    public TokenSettingsControllerTests()
    {
        _controller = new TokenSettingsController(_service.Object, Mock.Of<ILogger<TokenSettingsController>>());
    }

    [Fact(DisplayName = "When token settings load, GetTokenSettings returns Ok.")]
    public async Task GetTokenSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new TokenSettings { AccessTokenExpiryHours = 2 };
        _service.Setup(s => s.GetTokenSettingsAsync()).ReturnsAsync(settings);

        // Act
        var result = await _controller.GetTokenSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(settings);
    }

    [Fact(DisplayName = "When update succeeds, UpdateTokenSettings returns Ok.")]
    public async Task UpdateTokenSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new TokenSettings { AccessTokenExpiryHours = 1 };
        _service.Setup(s => s.UpdateTokenSettingsAsync(settings)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateTokenSettings(settings);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
