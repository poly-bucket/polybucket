using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ThemeManagement.SetActiveTheme;

public class SetActiveThemeControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When activation fails, set active theme returns BadRequest.")]
    public async Task SetActiveTheme_Failure_ReturnsBadRequest()
    {
        // Arrange
        var response = new SetActiveThemeResponse { Success = false, Message = "missing" };
        _mediator.SetupSend<SetActiveThemeCommand, SetActiveThemeResponse>(response);
        var controller = new SetActiveThemeController(_mediator.Object);

        // Act
        var result = await controller.SetActiveTheme(99);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When activation succeeds, set active theme returns Ok.")]
    public async Task SetActiveTheme_Success_ReturnsOk()
    {
        // Arrange
        var response = new SetActiveThemeResponse { Success = true, Message = "ok" };
        _mediator.SetupSend<SetActiveThemeCommand, SetActiveThemeResponse>(response);
        var controller = new SetActiveThemeController(_mediator.Object);

        // Act
        var result = await controller.SetActiveTheme(1);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
