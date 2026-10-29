using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Logout.Domain;
using PolyBucket.Api.Features.Authentication.Logout.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Http;

public class LogoutCommandControllerTests
{
    private readonly Mock<IAuthenticationRepository> _authRepositoryMock;
    private readonly LogoutController _controller;

    public LogoutCommandControllerTests()
    {
        _authRepositoryMock = new Mock<IAuthenticationRepository>();
        var handler = new LogoutCommandHandler(
            _authRepositoryMock.Object,
            Mock.Of<ILogger<LogoutCommandHandler>>());

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;

        _controller = new LogoutController(handler, Mock.Of<ILogger<LogoutController>>())
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact(DisplayName = "When logout is called with a valid refresh token, the token is revoked and NoContent is returned.")]
    public async Task Logout_ValidToken_ShouldRevokeAndReturnNoContent()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = "valid-refresh-token" };
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = command.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            User = new User { Email = "user@example.com" }
        };

        _authRepositoryMock
            .Setup(r => r.GetRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        _authRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(command.RefreshToken, "User logged out", It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        _authRepositoryMock.Verify(
            r => r.RevokeRefreshTokenAsync(command.RefreshToken, "User logged out", It.IsAny<string>()),
            Times.Once);
    }

    [Fact(DisplayName = "When logout is called without a refresh token, BadRequest is returned.")]
    public async Task Logout_MissingToken_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = "" };

        // Act
        var result = await _controller.Logout(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
