using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.RevokeAllSessions.Domain;
using PolyBucket.Api.Features.Authentication.RevokeAllSessions.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.RevokeAllSessions.Http;

public class RevokeAllSessionsControllerTests
{
    private readonly Mock<IRevokeAllSessionsService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the user is signed in, RevokeAll revokes all refresh tokens and returns Ok.")]
    public async Task RevokeAll_ValidUser_ReturnsOk()
    {
        // Arrange
        _service
            .Setup(s => s.RevokeAllSessionsAsync(_userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = new RevokeAllSessionsController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RevokeAll(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<RevokeAllSessionsResponse>().Success.ShouldBeTrue();
        _service.Verify(s => s.RevokeAllSessionsAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the token has no user id, RevokeAll returns Unauthorized.")]
    public async Task RevokeAll_NoUserClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await new RevokeAllSessionsController(_service.Object).WithUser(null).RevokeAll(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.Verify(s => s.RevokeAllSessionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
