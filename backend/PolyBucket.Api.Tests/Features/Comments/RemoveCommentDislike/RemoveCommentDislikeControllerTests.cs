using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.LikeComment.Domain;
using PolyBucket.Api.Features.Comments.RemoveCommentDislike.Domain;
using PolyBucket.Api.Features.Comments.RemoveCommentDislike.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.RemoveCommentDislike;

public class RemoveCommentDislikeControllerTests
{
    private readonly Mock<IRemoveCommentDislikeService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a dislike is removed, the controller returns Ok.")]
    public async Task RemoveDislike_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.RemoveReactionAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommentReactionOutcome(CommentReactionChange.Unchanged, 0, 1, null));
        var controller = new RemoveCommentDislikeController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.RemoveDislike(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
