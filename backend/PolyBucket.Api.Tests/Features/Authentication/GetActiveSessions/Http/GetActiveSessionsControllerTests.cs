using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.GetActiveSessions.Domain;
using PolyBucket.Api.Features.Authentication.GetActiveSessions.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.GetActiveSessions.Http;

public class GetActiveSessionsControllerTests
{
    private readonly Mock<IGetActiveSessionsService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When sessions exist, GetActiveSessions returns Ok with session summaries.")]
    public async Task GetActiveSessions_ValidUser_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expires = DateTime.UtcNow.AddDays(7);
        _service
            .Setup(s => s.GetActiveSessionsAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetActiveSessionsResponse
            {
                Sessions =
                [
                    new SessionSummaryDto
                    {
                        SessionId = sessionId,
                        CreatedAt = DateTime.UtcNow.AddHours(-1),
                        ExpiresAt = expires,
                        CreatedByIp = "10.0.0.1"
                    }
                ]
            });
        var controller = new GetActiveSessionsController(_service.Object).WithUser(_userId);

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
        var result = await new GetActiveSessionsController(_service.Object).WithUser(null).GetActiveSessions(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.Verify(s => s.GetActiveSessionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
