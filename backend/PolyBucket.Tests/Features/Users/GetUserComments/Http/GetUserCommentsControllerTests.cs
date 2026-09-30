using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.GetUserComments.Domain;
using PolyBucket.Api.Features.Users.GetUserComments.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserComments.Http;

public class GetUserCommentsControllerTests
{
    private readonly Mock<IGetUserCommentsService> _service = new();
    private readonly GetUserCommentsController _controller;

    public GetUserCommentsControllerTests()
    {
        _controller = new GetUserCommentsController(_service.Object, Mock.Of<ILogger<GetUserCommentsController>>());
    }

    [Fact(DisplayName = "When page is less than 1, GetUserComments returns BadRequest.")]
    public async Task GetUserComments_InvalidPage_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetUserComments(Guid.NewGuid(), page: 0);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _service.Verify(s => s.GetUserCommentsAsync(It.IsAny<GetUserCommentsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When page size is out of range, GetUserComments returns BadRequest.")]
    public async Task GetUserComments_InvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetUserComments(Guid.NewGuid(), pageSize: 200);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the query succeeds, GetUserComments returns Ok.")]
    public async Task GetUserComments_Success_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var payload = new GetUserCommentsResult { TotalCount = 1 };
        _service.Setup(s => s.GetUserCommentsAsync(It.IsAny<GetUserCommentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        var result = await _controller.GetUserComments(userId);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(payload);
    }
}
