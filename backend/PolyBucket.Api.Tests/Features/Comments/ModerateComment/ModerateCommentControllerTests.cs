using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.ModerateComment.Domain;
using PolyBucket.Api.Features.Comments.ModerateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.ModerateComment;

public class ModerateCommentControllerTests
{
    private readonly Mock<IModerateCommentService> _service = new();

    [Fact(DisplayName = "When moderation succeeds, the controller returns Ok.")]
    public async Task ModerateComment_Success_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.ModerateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), "spam", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new ModerateCommentController(_service.Object).WithUser(Guid.NewGuid(), "Moderator");

        // Act
        var result = await controller.ModerateComment(Guid.NewGuid(), new ModerateCommentRequest { Reason = "spam" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
