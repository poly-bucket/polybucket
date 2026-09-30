using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Users.UnbanUser.Domain;
using PolyBucket.Api.Features.Users.UnbanUser.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UnbanUser.Http;

public class UnbanUserControllerTests
{
    private readonly Mock<IUnbanUserService> _service = new();

    [Fact(DisplayName = "When unban succeeds, the controller returns Ok.")]
    public async Task UnbanUser_Valid_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service.Setup(s => s.UnbanUserAsync(userId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new UnbanUserController(_service.Object);

        // Act
        var result = await controller.UnbanUser(userId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the user is missing, unban returns NotFound.")]
    public async Task UnbanUser_Missing_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service
            .Setup(s => s.UnbanUserAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));
        var controller = new UnbanUserController(_service.Object);

        // Act
        var result = await controller.UnbanUser(userId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the user is not banned, unban returns BadRequest.")]
    public async Task UnbanUser_NotBanned_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service
            .Setup(s => s.UnbanUserAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User is not banned"));
        var controller = new UnbanUserController(_service.Object);

        // Act
        var result = await controller.UnbanUser(userId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
