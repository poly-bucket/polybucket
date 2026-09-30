using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.CreateComment.Http;
using PolyBucket.Api.Features.Comments.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.CreateComment;

public class CreateCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Mock<ICommentResponseMapper> _mapper = new();
    private readonly Guid _userId = Guid.NewGuid();

    private CreateCommentController CreateController(Guid? userId) =>
        new CreateCommentController(_plugin.Object, _mapper.Object).WithUser(userId);

    private static CreateCommentRequest Request(Guid? parentId = null) => new()
    {
        Target = CommentTarget.ForModel(Guid.NewGuid()),
        Content = "Printed great at 0.2mm",
        ParentCommentId = parentId
    };

    [Fact(DisplayName = "When a signed-in user comments, the created comment is returned.")]
    public async Task CreateComment_Valid_ReturnsOk()
    {
        // Arrange
        var request = Request();
        var comment = new EnhancedComment { Id = Guid.NewGuid(), AuthorId = _userId, Content = request.Content };
        _plugin.Setup(p => p.AddCommentAsync(request.Target, _userId, request.Content, null)).ReturnsAsync(comment);
        _mapper.Setup(m => m.MapAsync(It.IsAny<IReadOnlyList<EnhancedComment>>(), _userId, false, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommentResponse> { new() { Id = comment.Id, Content = comment.Content } });

        // Act
        var result = await CreateController(_userId).CreateComment(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentResponse>().Id.ShouldBe(comment.Id);
    }

    [Fact(DisplayName = "When the target or parent is invalid, the controller returns BadRequest with the reason.")]
    public async Task CreateComment_Invalid_ReturnsBadRequest()
    {
        // Arrange
        var request = Request(Guid.NewGuid());
        _plugin.Setup(p => p.AddCommentAsync(It.IsAny<CommentTarget>(), _userId, It.IsAny<string>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Parent comment not found"));

        // Act
        var result = await CreateController(_userId).CreateComment(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task CreateComment_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(null);

        // Act
        var result = await controller.CreateComment(Request(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
        _plugin.VerifyNoOtherCalls();
    }
}
