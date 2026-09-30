using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.GetComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.GetComment;

public class GetCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Mock<ICommentResponseMapper> _mapper = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the comment exists, it is returned.")]
    public async Task Get_Existing_ReturnsOk()
    {
        // Arrange
        var comment = new EnhancedComment { Id = Guid.NewGuid(), Content = "hi" };
        _plugin.Setup(p => p.GetCommentByIdAsync(comment.Id)).ReturnsAsync(comment);
        _mapper.Setup(m => m.MapAsync(It.IsAny<IReadOnlyList<EnhancedComment>>(), _userId, false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommentResponse> { new() { Id = comment.Id } });
        var controller = new GetCommentController(_plugin.Object, _mapper.Object).WithUser(_userId);

        // Act
        var result = await controller.GetComment(comment.Id, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentResponse>().Id.ShouldBe(comment.Id);
    }

    [Fact(DisplayName = "When the comment is hidden and the caller is not a moderator, the controller returns NotFound.")]
    public async Task Get_HiddenForUser_ReturnsNotFound()
    {
        // Arrange
        var comment = new EnhancedComment { Id = Guid.NewGuid(), IsHidden = true };
        _plugin.Setup(p => p.GetCommentByIdAsync(comment.Id)).ReturnsAsync(comment);
        var controller = new GetCommentController(_plugin.Object, _mapper.Object).WithUser(_userId);

        // Act
        var result = await controller.GetComment(comment.Id, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns NotFound.")]
    public async Task Get_Missing_ReturnsNotFound()
    {
        // Arrange
        var controller = new GetCommentController(_plugin.Object, _mapper.Object).WithUser(_userId);

        // Act
        var result = await controller.GetComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
