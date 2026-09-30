using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Notifications.MarkNotificationRead.Domain;
using PolyBucket.Api.Features.Notifications.MarkNotificationRead.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Notifications;

public class MarkNotificationReadControllerTests
{
    private readonly Mock<IMarkNotificationReadService> _service = new();

    private MarkNotificationReadController CreateController(Guid? userId) =>
        new(_service.Object) { ControllerContext = NotificationControllerTestContext.For(userId) };

    [Fact(DisplayName = "Marking an owned notification read responds 204.")]
    public async Task MarkRead_Owned_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        _service.Setup(s => s.MarkReadAsync(userId, notificationId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = CreateController(userId);

        // Act
        var result = await controller.MarkRead(notificationId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "Marking a missing or someone else's notification responds 404.")]
    public async Task MarkRead_NotOwned_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        _service.Setup(s => s.MarkReadAsync(userId, notificationId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var controller = CreateController(userId);

        // Act
        var result = await controller.MarkRead(notificationId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "A caller without a user id claim gets 401.")]
    public async Task MarkRead_NoUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.MarkRead(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
