using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserById.Domain;
using PolyBucket.Api.Features.Users.GetUserById.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserById.Http;

public class GetUserByIdControllerTests
{
    private readonly Mock<IGetUserByIdService> _service = new();
    private readonly Mock<ILogger<GetUserByIdController>> _logger = new();

    [Fact(DisplayName = "When the user exists, get user by id returns Ok.")]
    public async Task GetUserById_Found_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new GetUserByIdResult { Username = "alice", Email = "alice@example.com" };
        _service.Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var controller = new GetUserByIdController(_service.Object, _logger.Object);

        // Act
        var result = await controller.GetUserById(userId, CancellationToken.None);

        // Assert
        result.Value.ShouldBe(user);
    }

    [Fact(DisplayName = "When the user is missing, get user by id returns NotFound.")]
    public async Task GetUserById_Missing_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service.Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());
        var controller = new GetUserByIdController(_service.Object, _logger.Object);

        // Act
        var result = await controller.GetUserById(userId, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }
}
