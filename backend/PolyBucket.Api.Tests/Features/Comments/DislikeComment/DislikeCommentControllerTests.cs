using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.DislikeComment.Domain;
using PolyBucket.Api.Features.Comments.DislikeComment.Http;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Database.Entities.Comments;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.DislikeComment;

public class DislikeCommentControllerTests
{
    private readonly Mock<IDislikeCommentService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a comment is disliked, the new counts are returned.")]
    public async Task Dislike_Existing_ReturnsCounts()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _service.Setup(s => s.ReactAsync(commentId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Applied, 0, 2, CommentReactionType.Dislike));
        var controller = new DislikeCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.DislikeComment(commentId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task Dislike_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new DislikeCommentController(_service.Object).WithUser(null);

        // Act
        var result = await controller.DislikeComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
