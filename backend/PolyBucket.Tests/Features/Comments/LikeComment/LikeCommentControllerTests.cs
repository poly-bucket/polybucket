using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.LikeComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.LikeComment;

public class LikeCommentControllerTests
{
    private readonly Mock<ICommentReactionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a comment is liked, the new counts and the caller's like are returned.")]
    public async Task Like_Existing_ReturnsCounts()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _service.Setup(s => s.ReactAsync(commentId, _userId, CommentReactionType.Like, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Applied, 4, 0, CommentReactionType.Like));
        var controller = new LikeCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.LikeComment(commentId, CancellationToken.None);

        // Assert
        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentReactionResponse>();
        body.Likes.ShouldBe(4);
        body.UserHasLiked.ShouldBeTrue();
        body.Changed.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns NotFound.")]
    public async Task Like_Missing_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.ReactAsync(It.IsAny<Guid>(), _userId, CommentReactionType.Like, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommentReactionOutcome.NotFound);
        var controller = new LikeCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.LikeComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task Like_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new LikeCommentController(_service.Object).WithUser(null);

        // Act
        var result = await controller.LikeComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
