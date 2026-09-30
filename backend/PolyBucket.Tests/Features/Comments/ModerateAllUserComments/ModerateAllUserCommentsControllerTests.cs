using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.ModerateAllUserComments.Http;
using PolyBucket.Api.Features.Comments.ModerateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.ModerateAllUserComments;

public class ModerateAllUserCommentsControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Guid _moderatorId = Guid.NewGuid();

    [Fact(DisplayName = "When a moderator hides all of a user's comments, the controller returns Ok.")]
    public async Task ModerateAll_Valid_ReturnsOk()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        _plugin.Setup(p => p.ModerateAllCommentsForUserAsync(targetUserId, _moderatorId, "spam")).ReturnsAsync(true);
        var controller = new ModerateAllUserCommentsController(_plugin.Object).WithUser(_moderatorId, "Moderator");

        // Act
        var result = await controller.ModerateAllUserComments(targetUserId, new ModerateCommentRequest { Reason = "spam" });

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the comments cannot be moderated, the controller returns BadRequest.")]
    public async Task ModerateAll_Failed_ReturnsBadRequest()
    {
        // Arrange
        var controller = new ModerateAllUserCommentsController(_plugin.Object).WithUser(_moderatorId, "Moderator");

        // Act
        var result = await controller.ModerateAllUserComments(Guid.NewGuid(), new ModerateCommentRequest { Reason = "spam" });

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
