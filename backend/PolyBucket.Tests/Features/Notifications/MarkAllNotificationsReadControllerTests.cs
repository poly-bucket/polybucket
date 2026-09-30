using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Domain;
using PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Notifications;

public class MarkAllNotificationsReadControllerTests
{
    private readonly Mock<IMarkAllNotificationsReadService> _service = new();

    private MarkAllNotificationsReadController CreateController(Guid? userId) =>
        new(_service.Object) { ControllerContext = NotificationControllerTestContext.For(userId) };

    [Fact(DisplayName = "Marking all read returns how many notifications changed.")]
    public async Task MarkAllRead_SignedIn_ReturnsUpdatedCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service.Setup(s => s.MarkAllReadAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(7);
        var controller = CreateController(userId);

        // Act
        var result = await controller.MarkAllRead(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(new MarkAllNotificationsReadResponse(7));
    }

    [Fact(DisplayName = "A caller without a user id claim gets 401.")]
    public async Task MarkAllRead_NoUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.MarkAllRead(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
