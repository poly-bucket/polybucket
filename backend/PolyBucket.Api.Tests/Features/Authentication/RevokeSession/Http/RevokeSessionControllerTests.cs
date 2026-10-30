using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.RevokeSession.Domain;
using PolyBucket.Api.Features.Authentication.RevokeSession.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.RevokeSession.Http;

public class RevokeSessionControllerTests
{
    private readonly Mock<IRevokeSessionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the session exists, RevokeSession returns Ok.")]
    public async Task RevokeSession_Found_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _service
            .Setup(s => s.RevokeSessionAsync(_userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevokeSessionResult.Success());
        var controller = new RevokeSessionController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<RevokeSessionResponse>().Success.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the session is missing, RevokeSession returns NotFound.")]
    public async Task RevokeSession_Missing_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _service
            .Setup(s => s.RevokeSessionAsync(_userId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevokeSessionResult.NotFound());
        var controller = new RevokeSessionController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the token has no user id, RevokeSession returns Unauthorized.")]
    public async Task RevokeSession_NoUserClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await new RevokeSessionController(_service.Object)
            .WithUser(null)
            .RevokeSession(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }
}
