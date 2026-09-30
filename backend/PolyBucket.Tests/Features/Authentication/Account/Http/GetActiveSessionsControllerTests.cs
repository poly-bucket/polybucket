using System;
using System.Collections.Generic;
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

public class GetActiveSessionsControllerTests
{
    private readonly Mock<IAuthenticationRepository> _authRepository = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When sessions exist, GetActiveSessions returns Ok with session summaries.")]
    public async Task GetActiveSessions_ValidUser_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expires = DateTime.UtcNow.AddDays(7);
        _authRepository
            .Setup(r => r.GetActiveRefreshTokensForUserAsync(_userId))
            .ReturnsAsync(new List<RefreshToken>
            {
                new()
                {
                    Id = sessionId,
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    ExpiresAt = expires,
                    CreatedByIp = "10.0.0.1"
                }
            });
        var controller = new GetActiveSessionsController(_authRepository.Object).WithUser(_userId);

        // Act
        var result = await controller.GetActiveSessions(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = ok.Value.ShouldBeOfType<GetActiveSessionsResponse>();
        response.Sessions.Count.ShouldBe(1);
        response.Sessions[0].SessionId.ShouldBe(sessionId);
        response.Sessions[0].CreatedByIp.ShouldBe("10.0.0.1");
    }

    [Fact(DisplayName = "When the token has no user id, GetActiveSessions returns Unauthorized.")]
    public async Task GetActiveSessions_NoUserClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await new GetActiveSessionsController(_authRepository.Object).WithUser(null).GetActiveSessions(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _authRepository.Verify(r => r.GetActiveRefreshTokensForUserAsync(It.IsAny<Guid>()), Times.Never);
    }
}
