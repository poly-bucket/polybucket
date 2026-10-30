using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.ModerateAllUserComments.Domain;
using PolyBucket.Api.Features.Comments.ModerateAllUserComments.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.ModerateAllUserComments;

public class ModerateAllUserCommentsControllerTests
{
    private readonly Mock<IModerateAllUserCommentsService> _service = new();

    [Fact(DisplayName = "When bulk moderation succeeds, the controller returns Ok.")]
    public async Task ModerateAllUserComments_Success_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.ModerateAllAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), "spam", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new ModerateAllUserCommentsController(_service.Object).WithUser(Guid.NewGuid(), "Moderator");

        // Act
        var result = await controller.ModerateAllUserComments(Guid.NewGuid(), new ModerateAllUserCommentsRequest { Reason = "spam" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }
}
