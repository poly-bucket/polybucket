using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.GetCommentStatistics.Domain;
using PolyBucket.Api.Features.Comments.GetCommentStatistics.Http;
using PolyBucket.Api.Features.Comments.Plugins;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetCommentStatistics;

public class GetCommentStatisticsControllerTests
{
    private readonly Mock<IGetCommentStatisticsService> _service = new();

    [Fact(DisplayName = "When the target is valid, statistics are returned.")]
    public async Task GetStatistics_Valid_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.GetAsync("model", modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCommentStatisticsResult(new CommentStatistics(), null));
        var controller = new GetCommentStatisticsController(_service.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetStatistics("model", modelId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
