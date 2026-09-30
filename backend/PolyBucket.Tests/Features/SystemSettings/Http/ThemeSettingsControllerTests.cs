using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class ThemeSettingsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly ThemeSettingsController _controller;

    public ThemeSettingsControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When theme settings load successfully, GetThemeSettings returns Ok.")]
    public async Task GetThemeSettings_Success_ReturnsOk()
    {
        // Arrange
        var response = new GetThemeSettingsResponse { Success = true };
        _mediator.SetupSend<GetThemeSettingsQuery, GetThemeSettingsResponse>(response);

        // Act
        var result = await _controller.GetThemeSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When theme settings fail to load, GetThemeSettings returns BadRequest.")]
    public async Task GetThemeSettings_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new GetThemeSettingsResponse { Success = false };
        _mediator.SetupSend<GetThemeSettingsQuery, GetThemeSettingsResponse>(response);

        // Act
        var result = await _controller.GetThemeSettings();

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }
}
