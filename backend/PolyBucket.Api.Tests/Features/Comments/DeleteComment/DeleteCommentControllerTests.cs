using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.DeleteComment.Domain;
using PolyBucket.Api.Features.Comments.DeleteComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.DeleteComment;

public class DeleteCommentControllerTests
{
    private readonly Mock<IDeleteCommentService> _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When deletion succeeds, the controller returns NoContent.")]
    public async Task DeleteComment_Success_ReturnsNoContent()
    {
        // Arrange
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), _userId, false, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new DeleteCommentController(_service.Object).WithUser(_userId);

        // Act
        var result = await controller.DeleteComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the controller returns Unauthorized.")]
    public async Task DeleteComment_NoUser_ReturnsUnauthorized()
    {
        // Arrange
        var controller = new DeleteCommentController(_service.Object).WithUser(null);

        // Act
        var result = await controller.DeleteComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }
}
