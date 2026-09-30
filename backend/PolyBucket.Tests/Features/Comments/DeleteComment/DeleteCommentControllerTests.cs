using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.DeleteComment.Http;
using PolyBucket.Api.Features.Comments.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.DeleteComment;

public class DeleteCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When the author deletes their comment, the controller returns NoContent.")]
    public async Task Delete_Owner_ReturnsNoContent()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _plugin.Setup(p => p.DeleteCommentAsync(commentId, _userId, false)).ReturnsAsync(true);
        var controller = new DeleteCommentController(_plugin.Object).WithUser(_userId);

        // Act
        var result = await controller.DeleteComment(commentId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When an admin deletes a comment, the admin flag is passed to the plugin.")]
    public async Task Delete_Admin_PassesAdminFlag()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _plugin.Setup(p => p.DeleteCommentAsync(commentId, _userId, true)).ReturnsAsync(true);
        var controller = new DeleteCommentController(_plugin.Object).WithUser(_userId, "Admin");

        // Act
        var result = await controller.DeleteComment(commentId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When the comment cannot be deleted, the controller returns NotFound.")]
    public async Task Delete_NotAllowed_ReturnsNotFound()
    {
        // Arrange
        var controller = new DeleteCommentController(_plugin.Object).WithUser(_userId);

        // Act
        var result = await controller.DeleteComment(Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
