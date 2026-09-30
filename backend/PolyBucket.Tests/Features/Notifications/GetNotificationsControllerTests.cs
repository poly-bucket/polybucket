using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Notifications.GetNotifications.Domain;
using PolyBucket.Api.Features.Notifications.GetNotifications.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Notifications;

public class GetNotificationsControllerTests
{
    private readonly Mock<IGetNotificationsService> _service = new();

    private GetNotificationsController CreateController(Guid? userId) =>
        new(_service.Object) { ControllerContext = NotificationControllerTestContext.For(userId) };

    [Fact(DisplayName = "A signed-in user gets their page of notifications.")]
    public async Task GetNotifications_SignedIn_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var response = new GetNotificationsResponse { TotalCount = 3, UnreadCount = 1, Page = 2, PageSize = 10, TotalPages = 1 };
        _service.Setup(s => s.GetNotificationsAsync(userId, true, 2, 10, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = CreateController(userId);

        // Act
        var result = await controller.GetNotifications(2, 10, true, CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(response);
    }

    [Theory(DisplayName = "Out-of-range paging values are rejected with 400.")]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task GetNotifications_InvalidPaging_ReturnsBadRequest(int page, int pageSize)
    {
        // Arrange
        var controller = CreateController(Guid.NewGuid());

        // Act
        var result = await controller.GetNotifications(page, pageSize, false, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _service.Verify(s => s.GetNotificationsAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "A caller without a user id claim gets 401.")]
    public async Task GetNotifications_NoUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.GetNotifications(1, 20, false, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
