using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Http;
using PolyBucket.Api.Features.Comments.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.DeleteAllCommentsForTarget;

public class DeleteAllCommentsForTargetControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Guid _adminId = Guid.NewGuid();

    [Fact(DisplayName = "When an admin clears a model's comments, the controller returns Ok.")]
    public async Task DeleteAll_Valid_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        _plugin.Setup(p => p.DeleteAllCommentsForTargetAsync(It.Is<CommentTarget>(t => t.TargetId == modelId), _adminId)).ReturnsAsync(true);
        var controller = new DeleteAllCommentsForTargetController(_plugin.Object).WithUser(_adminId, "Admin");

        // Act
        var result = await controller.DeleteAllCommentsForTarget("model", modelId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the target type is unknown, the controller returns BadRequest without deleting.")]
    public async Task DeleteAll_InvalidType_ReturnsBadRequest()
    {
        // Arrange
        var controller = new DeleteAllCommentsForTargetController(_plugin.Object).WithUser(_adminId, "Admin");

        // Act
        var result = await controller.DeleteAllCommentsForTarget("printer", Guid.NewGuid());

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _plugin.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "The controller is restricted to admins.")]
    public void Controller_RequiresAdminRole()
    {
        // Arrange
        var type = typeof(DeleteAllCommentsForTargetController);

        // Act
        var authorize = type.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        authorize!.Roles.ShouldBe("Admin");
    }
}
