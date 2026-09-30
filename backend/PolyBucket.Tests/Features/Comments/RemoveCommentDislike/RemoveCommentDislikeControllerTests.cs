using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.RemoveCommentDislike.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.RemoveCommentDislike;

public class RemoveCommentDislikeControllerTests
{
    private readonly Mock<ICommentReactionService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a dislike is removed, the reduced count is returned.")]
    public async Task RemoveDislike_Disliked_ReturnsCounts()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _service.Setup(s => s.RemoveReactionAsync(commentId, _userId, CommentReactionType.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Applied, 0, 0, null));
        var controller = new RemoveCommentDislikeController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RemoveDislike(commentId, CancellationToken.None);

        // Assert
        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentReactionResponse>();
        body.Dislikes.ShouldBe(0);
        body.UserHasDisliked.ShouldBeFalse();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns NotFound.")]
    public async Task RemoveDislike_Missing_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.RemoveReactionAsync(It.IsAny<Guid>(), _userId, CommentReactionType.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommentReactionOutcome.NotFound);
        var controller = new RemoveCommentDislikeController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RemoveDislike(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
