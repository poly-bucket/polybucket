using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Api.Features.Comments.UpdateComment.Domain;
using PolyBucket.Api.Features.Comments.UpdateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.UpdateComment;

public class UpdateCommentControllerTests
{
    private readonly Mock<IUpdateCommentService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the caller updates their comment, the updated comment is returned.")]
    public async Task UpdateComment_Valid_ReturnsOk()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var response = new CommentResponse { Id = commentId, Content = "updated" };
        _service.Setup(s => s.UpdateAsync(commentId, _userId, "updated", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateCommentResult(response, null, false));
        var controller = new UpdateCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.UpdateComment(commentId, new UpdateCommentContentRequest { Content = "updated" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task UpdateComment_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new UpdateCommentController(_service.Object).WithUser(null);

        // Act
        var result = await controller.UpdateComment(Guid.NewGuid(), new UpdateCommentContentRequest { Content = "x" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
