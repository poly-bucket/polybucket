using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Domain;
using PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Notifications;

public class GetUnreadNotificationCountControllerTests
{
    private readonly Mock<IGetUnreadNotificationCountService> _service = new();

    private GetUnreadNotificationCountController CreateController(Guid? userId) =>
        new(_service.Object) { ControllerContext = NotificationControllerTestContext.For(userId) };

    [Fact(DisplayName = "A signed-in user gets their unread count and the response is not cached.")]
    public async Task GetUnreadCount_SignedIn_ReturnsCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service.Setup(s => s.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(4);
        var controller = CreateController(userId);

        // Act
        var result = await controller.GetUnreadCount(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(new UnreadNotificationCountResponse(4));
        controller.Response.Headers.CacheControl.ToString().ShouldBe("no-store");
    }

    [Fact(DisplayName = "A caller without a user id claim gets 401.")]
    public async Task GetUnreadCount_NoUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.GetUnreadCount(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
