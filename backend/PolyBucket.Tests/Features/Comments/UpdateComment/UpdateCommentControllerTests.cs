using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.UpdateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.UpdateComment;

public class UpdateCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Mock<ICommentResponseMapper> _mapper = new();
    private readonly Guid _userId = Guid.NewGuid();

    private UpdateCommentController CreateController() => new UpdateCommentController(_plugin.Object, _mapper.Object).WithUser(_userId);

    [Fact(DisplayName = "When the author edits their comment, the updated comment is returned.")]
    public async Task Update_Owner_ReturnsOk()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _plugin.Setup(p => p.UpdateCommentAsync(commentId, _userId, "edited")).ReturnsAsync(new EnhancedComment { Id = commentId, Content = "edited" });
        _mapper.Setup(m => m.MapAsync(It.IsAny<IReadOnlyList<EnhancedComment>>(), _userId, false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommentResponse> { new() { Id = commentId, Content = "edited", IsEdited = true } });

        // Act
        var result = await CreateController().UpdateComment(commentId, new UpdateCommentContentRequest { Content = "edited" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentResponse>().IsEdited.ShouldBeTrue();
    }

    [Fact(DisplayName = "When someone else's comment is edited, the controller returns Forbid.")]
    public async Task Update_NotOwner_ReturnsForbid()
    {
        // Arrange
        _plugin.Setup(p => p.UpdateCommentAsync(It.IsAny<Guid>(), _userId, It.IsAny<string>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await CreateController().UpdateComment(Guid.NewGuid(), new UpdateCommentContentRequest { Content = "x" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns BadRequest.")]
    public async Task Update_Missing_ReturnsBadRequest()
    {
        // Arrange
        _plugin.Setup(p => p.UpdateCommentAsync(It.IsAny<Guid>(), _userId, It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("Comment not found"));

        // Act
        var result = await CreateController().UpdateComment(Guid.NewGuid(), new UpdateCommentContentRequest { Content = "x" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
