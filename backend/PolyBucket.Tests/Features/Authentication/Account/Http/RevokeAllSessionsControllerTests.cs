using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.Account.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Account.Http;

public class RevokeAllSessionsControllerTests
{
    private readonly Mock<IAuthenticationRepository> _authRepository = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the user is signed in, RevokeAll revokes all refresh tokens and returns Ok.")]
    public async Task RevokeAll_ValidUser_ReturnsOk()
    {
        // Arrange
        _authRepository
            .Setup(r => r.RevokeAllRefreshTokensForUserAsync(_userId, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var controller = new RevokeAllSessionsController(_authRepository.Object).WithUser(_userId);

        // Act
        var result = await controller.RevokeAll(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<RevokeAllSessionsResponse>().Success.ShouldBeTrue();
        _authRepository.Verify(
            r => r.RevokeAllRefreshTokensForUserAsync(_userId, "User revoked all sessions", "127.0.0.1"),
            Times.Once);
    }

    [Fact(DisplayName = "When the token has no user id, RevokeAll returns Unauthorized.")]
    public async Task RevokeAll_NoUserClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await new RevokeAllSessionsController(_authRepository.Object).WithUser(null).RevokeAll(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _authRepository.Verify(
            r => r.RevokeAllRefreshTokensForUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
