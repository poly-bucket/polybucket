using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.DislikeComment.Http;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.DislikeComment;

public class DislikeCommentControllerTests
{
    private readonly Mock<ICommentReactionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a liked comment is disliked, the switched counts are returned.")]
    public async Task Dislike_Existing_ReturnsCounts()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _service.Setup(s => s.ReactAsync(commentId, _userId, CommentReactionType.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Applied, 0, 1, CommentReactionType.Dislike));
        var controller = new DislikeCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.DislikeComment(commentId, CancellationToken.None);

        // Assert
        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentReactionResponse>();
        body.Dislikes.ShouldBe(1);
        body.UserHasDisliked.ShouldBeTrue();
        body.UserHasLiked.ShouldBeFalse();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns NotFound.")]
    public async Task Dislike_Missing_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.ReactAsync(It.IsAny<Guid>(), _userId, CommentReactionType.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommentReactionOutcome.NotFound);
        var controller = new DislikeCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.DislikeComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
