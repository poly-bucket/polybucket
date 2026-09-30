using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.GetModeratedComments.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.GetModeratedComments;

public class GetModeratedCommentsControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Mock<ICommentResponseMapper> _mapper = new();

    [Fact(DisplayName = "When a moderator lists moderated comments, the mapped comments are returned.")]
    public async Task Get_ReturnsMappedComments()
    {
        // Arrange
        var comment = new EnhancedComment { Id = Guid.NewGuid(), IsModerated = true };
        _plugin.Setup(p => p.GetModeratedCommentsAsync(1, 20)).ReturnsAsync(new[] { comment });
        _mapper.Setup(m => m.MapAsync(It.IsAny<IReadOnlyList<EnhancedComment>>(), It.IsAny<Guid?>(), It.IsAny<bool>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommentResponse> { new() { Id = comment.Id, IsModerated = true } });
        var controller = new GetModeratedCommentsController(_plugin.Object, _mapper.Object).WithUser(Guid.NewGuid(), "Moderator");

        // Act
        var result = await controller.GetModeratedComments();

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<List<CommentResponse>>().ShouldHaveSingleItem();
    }

    [Fact(DisplayName = "When the page size is too large, the controller returns BadRequest.")]
    public async Task Get_InvalidPaging_ReturnsBadRequest()
    {
        // Arrange
        var controller = new GetModeratedCommentsController(_plugin.Object, _mapper.Object).WithUser(Guid.NewGuid(), "Moderator");

        // Act
        var result = await controller.GetModeratedComments(1, 1000);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
