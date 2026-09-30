using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Users.BanUser.Domain;
using PolyBucket.Api.Features.Users.BanUser.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.BanUser.Http;

public class BanUserControllerTests
{
    private readonly Mock<IBanUserService> _service = new();
    private readonly Guid _adminId = Guid.NewGuid();

    private BanUserController CreateController(Guid? userId) =>
        new BanUserController(_service.Object).WithUser(userId);

    [Fact(DisplayName = "When ban succeeds, the controller returns Ok.")]
    public async Task BanUser_Valid_ReturnsOk()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        _service
            .Setup(s => s.BanUserAsync(targetId, _adminId, "abuse", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController(_adminId);

        // Act
        var result = await controller.BanUser(targetId, new BanUserRequest { Reason = "abuse" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the user is missing, ban returns NotFound.")]
    public async Task BanUser_MissingUser_ReturnsNotFound()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        _service
            .Setup(s => s.BanUserAsync(targetId, _adminId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));
        var controller = CreateController(_adminId);

        // Act
        var result = await controller.BanUser(targetId, new BanUserRequest { Reason = "abuse" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When ban rules are violated, ban returns BadRequest.")]
    public async Task BanUser_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        _service
            .Setup(s => s.BanUserAsync(targetId, _adminId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot ban yourself"));
        var controller = CreateController(_adminId);

        // Act
        var result = await controller.BanUser(targetId, new BanUserRequest { Reason = "abuse" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, ban returns Unauthorized.")]
    public async Task BanUser_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.BanUser(Guid.NewGuid(), new BanUserRequest { Reason = "abuse" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
        _service.VerifyNoOtherCalls();
    }
}
