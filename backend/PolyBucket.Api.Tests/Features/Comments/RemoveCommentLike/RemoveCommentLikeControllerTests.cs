using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Http;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Api.Features.Comments.RemoveCommentLike.Domain;
using PolyBucket.Api.Features.Comments.RemoveCommentLike.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.RemoveCommentLike;

public class RemoveCommentLikeControllerTests
{
    private readonly Mock<IRemoveCommentLikeService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a like is removed, the controller returns Ok.")]
    public async Task RemoveLike_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.RemoveReactionAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Unchanged, 1, 0, null));
        var controller = new RemoveCommentLikeController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RemoveLike(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
