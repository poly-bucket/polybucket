using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Domain;
using PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Http;
using PolyBucket.Api.Features.Comments.Plugins;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetUserCommentStatistics;

public class GetUserCommentStatisticsControllerTests
{
    private readonly Mock<IGetUserCommentStatisticsService> _service = new();

    [Fact(DisplayName = "When the user id is valid, statistics are returned.")]
    public async Task GetUserStatistics_Valid_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _service.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserCommentStatistics { UserId = userId });
        var controller = new GetUserCommentStatisticsController(_service.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetUserStatistics(userId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
