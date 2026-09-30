using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.RemoveCommentLike.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.RemoveCommentLike;

public class RemoveCommentLikeControllerTests
{
    private readonly Mock<ICommentReactionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a like is removed, the reduced count is returned.")]
    public async Task RemoveLike_Liked_ReturnsCounts()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _service.Setup(s => s.RemoveReactionAsync(commentId, _userId, CommentReactionType.Like, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Applied, 2, 0, null));
        var controller = new RemoveCommentLikeController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RemoveLike(commentId, CancellationToken.None);

        // Assert
        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentReactionResponse>();
        body.Likes.ShouldBe(2);
        body.UserHasLiked.ShouldBeFalse();
    }

    [Fact(DisplayName = "When there was no like to remove, the controller still returns Ok with Changed false.")]
    public async Task RemoveLike_NotLiked_ReturnsUnchanged()
    {
        // Arrange
        _service.Setup(s => s.RemoveReactionAsync(It.IsAny<Guid>(), _userId, CommentReactionType.Like, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Unchanged, 2, 0, null));
        var controller = new RemoveCommentLikeController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RemoveLike(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentReactionResponse>().Changed.ShouldBeFalse();
    }
}
