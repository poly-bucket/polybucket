using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.CreateComment.Domain;
using PolyBucket.Api.Features.Comments.CreateComment.Http;
using PolyBucket.Api.Features.Comments.Plugins;
using PolyBucket.Database.Entities.Comments;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.CreateComment;

public class CreateCommentControllerTests
{
    private readonly Mock<ICreateCommentService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    private CreateCommentController CreateController(Guid? userId) =>
        new CreateCommentController(_service.Object).WithUser(userId);

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
        var response = new CommentResponse { Id = Guid.NewGuid(), Content = request.Content };
        _service.Setup(s => s.CreateAsync(_userId, request.Target, request.Content, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateCommentResult(response, null));

        // Act
        var result = await CreateController(_userId).CreateComment(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentResponse>().Id.ShouldBe(response.Id);
    }

    [Fact(DisplayName = "When the target or parent is invalid, the controller returns BadRequest with the reason.")]
    public async Task CreateComment_Invalid_ReturnsBadRequest()
    {
        // Arrange
        var request = Request(Guid.NewGuid());
        _service.Setup(s => s.CreateAsync(_userId, It.IsAny<CommentTarget>(), It.IsAny<string>(), It.IsAny<Guid?>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateCommentResult(null, "Parent comment not found"));

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
        _service.VerifyNoOtherCalls();
    }
}
