using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.ChangePassword.Domain;
using PolyBucket.Api.Features.Authentication.ChangePassword.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.ChangePassword.Http;

public class ChangePasswordControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private ChangePasswordController CreateController()
    {
        var controller = new ChangePasswordController(_mediator.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact(DisplayName = "When the password change succeeds, ChangePassword returns Ok.")]
    public async Task ChangePassword_Success_ReturnsOk()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        };
        var response = new ChangePasswordResponse { Success = true, Message = "Updated" };
        _mediator.SetupSend<ChangePasswordCommand, ChangePasswordResponse>(response);

        // Act
        var result = await CreateController().ChangePassword(command);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        command.Client.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When the handler reports failure, ChangePassword returns BadRequest.")]
    public async Task ChangePassword_Failure_ReturnsBadRequest()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        };
        _mediator.SetupSend<ChangePasswordCommand, ChangePasswordResponse>(
            new ChangePasswordResponse { Success = false, Message = "Wrong password" });

        // Act
        var result = await CreateController().ChangePassword(command);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When model binding fails, ChangePassword returns BadRequest without sending.")]
    public async Task ChangePassword_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("NewPassword", "Too short");

        // Act
        var result = await controller.ChangePassword(new ChangePasswordCommand());

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _mediator.Verify(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
