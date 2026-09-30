using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.UnmoderateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.UnmoderateComment;

public class UnmoderateCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Guid _moderatorId = Guid.NewGuid();

    [Fact(DisplayName = "When a moderator restores a comment, the controller returns Ok.")]
    public async Task Unmoderate_Existing_ReturnsOk()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _plugin.Setup(p => p.UnmoderateCommentAsync(commentId, _moderatorId)).ReturnsAsync(true);
        var controller = new UnmoderateCommentController(_plugin.Object).WithUser(_moderatorId, "Admin");

        // Act
        var result = await controller.UnmoderateComment(commentId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the comment does not exist, the controller returns NotFound.")]
    public async Task Unmoderate_Missing_ReturnsNotFound()
    {
        // Arrange
        var controller = new UnmoderateCommentController(_plugin.Object).WithUser(_moderatorId, "Admin");

        // Act
        var result = await controller.UnmoderateComment(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "The controller is restricted to admins and moderators.")]
    public void Controller_RequiresModeratorRole()
    {
        // Arrange
        var type = typeof(UnmoderateCommentController);

        // Act
        var authorize = type.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        authorize!.Roles.ShouldBe("Admin,Moderator");
    }
}
