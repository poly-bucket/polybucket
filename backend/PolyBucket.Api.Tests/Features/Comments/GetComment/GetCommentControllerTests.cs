using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.GetComment.Domain;
using PolyBucket.Api.Features.Comments.GetComment.Http;
using PolyBucket.Api.Features.Comments.Plugins;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetComment;

public class GetCommentControllerTests
{
    private readonly Mock<IGetCommentService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the comment exists, it is returned.")]
    public async Task GetComment_Found_ReturnsOk()
    {
        // Arrange
        var response = new CommentResponse { Id = Guid.NewGuid(), Content = "hi" };
        _service.Setup(s => s.GetAsync(response.Id, _userId, false, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCommentResult(response, false));
        var controller = new GetCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.GetComment(response.Id, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the comment is missing, the controller returns NotFound.")]
    public async Task GetComment_Missing_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.GetAsync(It.IsAny<Guid>(), _userId, false, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCommentResult(null, true));
        var controller = new GetCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.GetComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
