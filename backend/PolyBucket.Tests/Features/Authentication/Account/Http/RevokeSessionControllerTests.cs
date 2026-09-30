using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.Account.Http;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Account.Http;

public class RevokeSessionControllerTests
{
    private readonly Mock<IAuthenticationRepository> _authRepository = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the session exists, RevokeSession returns Ok.")]
    public async Task RevokeSession_Found_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _authRepository
            .Setup(r => r.GetActiveRefreshTokenByIdAsync(sessionId, _userId))
            .ReturnsAsync(new RefreshToken { Id = sessionId, UserId = _userId });
        _authRepository
            .Setup(r => r.RevokeRefreshTokenByIdAsync(sessionId, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var controller = new RevokeSessionController(_authRepository.Object).WithUser(_userId);

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
        _authRepository
            .Setup(r => r.GetActiveRefreshTokenByIdAsync(sessionId, _userId))
            .ReturnsAsync((RefreshToken?)null);
        var controller = new RevokeSessionController(_authRepository.Object).WithUser(_userId);

        // Act
        var result = await controller.RevokeSession(sessionId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the token has no user id, RevokeSession returns Unauthorized.")]
    public async Task RevokeSession_NoUserClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await new RevokeSessionController(_authRepository.Object)
            .WithUser(null)
            .RevokeSession(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }
}
