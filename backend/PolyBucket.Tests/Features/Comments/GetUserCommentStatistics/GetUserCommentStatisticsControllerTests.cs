using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.GetUserCommentStatistics;

public class GetUserCommentStatisticsControllerTests
{
    [Fact(DisplayName = "When a user's statistics are requested, they are returned.")]
    public async Task GetUserStatistics_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plugin = new Mock<IEnhancedCommentsPlugin>();
        plugin.Setup(p => p.GetUserCommentStatisticsAsync(userId)).ReturnsAsync(new UserCommentStatistics { UserId = userId, TotalComments = 3 });
        var controller = new GetUserCommentStatisticsController(plugin.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetUserStatistics(userId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<UserCommentStatistics>().TotalComments.ShouldBe(3);
    }
}
