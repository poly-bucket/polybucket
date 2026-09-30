using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.GetCommentStatistics.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.GetCommentStatistics;

public class GetCommentStatisticsControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();

    [Fact(DisplayName = "When the target type is valid, the statistics are returned.")]
    public async Task GetStatistics_Valid_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _plugin.Setup(p => p.GetCommentStatisticsAsync(It.Is<CommentTarget>(t => t.TargetId == modelId && t.TargetType == CommentTargetType.Model)))
            .ReturnsAsync(new CommentStatistics { TotalComments = 7 });
        var controller = new GetCommentStatisticsController(_plugin.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetStatistics("model", modelId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentStatistics>().TotalComments.ShouldBe(7);
    }

    [Fact(DisplayName = "When the target type is unknown, the controller returns BadRequest.")]
    public async Task GetStatistics_InvalidType_ReturnsBadRequest()
    {
        // Arrange
        var controller = new GetCommentStatisticsController(_plugin.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetStatistics("printer", Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
