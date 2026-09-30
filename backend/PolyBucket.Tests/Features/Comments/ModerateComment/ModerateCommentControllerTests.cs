using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.ModerateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.ModerateComment;

public class ModerateCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Guid _moderatorId = Guid.NewGuid();

    [Fact(DisplayName = "When a moderator hides a comment, the controller returns Ok.")]
    public async Task Moderate_Existing_ReturnsOk()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _plugin.Setup(p => p.ModerateCommentAsync(commentId, _moderatorId, "off-topic")).ReturnsAsync(true);
        var controller = new ModerateCommentController(_plugin.Object).WithUser(_moderatorId, "Moderator");

        // Act
        var result = await controller.ModerateComment(commentId, new ModerateCommentRequest { Reason = "off-topic" });

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns NotFound.")]
    public async Task Moderate_Missing_ReturnsNotFound()
    {
        // Arrange
        var controller = new ModerateCommentController(_plugin.Object).WithUser(_moderatorId, "Moderator");

        // Act
        var result = await controller.ModerateComment(Guid.NewGuid(), new ModerateCommentRequest { Reason = "x" });

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "The controller is restricted to admins and moderators.")]
    public void Controller_RequiresModeratorRole()
    {
        // Arrange
        var type = typeof(ModerateCommentController);

        // Act
        var authorize = type.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        authorize.ShouldNotBeNull();
        authorize!.Roles.ShouldBe("Admin,Moderator");
    }
}
