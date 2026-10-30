using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.UnmoderateComment.Domain;
using PolyBucket.Api.Features.Comments.UnmoderateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.UnmoderateComment;

public class UnmoderateCommentControllerTests
{
    private readonly Mock<IUnmoderateCommentService> _service = new();

    [Fact(DisplayName = "When unmoderation succeeds, the controller returns Ok.")]
    public async Task UnmoderateComment_Success_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.UnmoderateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new UnmoderateCommentController(_service.Object).WithUser(Guid.NewGuid(), "Moderator");

        // Act
        var result = await controller.UnmoderateComment(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
