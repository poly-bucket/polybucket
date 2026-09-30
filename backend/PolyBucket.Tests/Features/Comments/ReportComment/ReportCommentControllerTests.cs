using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.ReportComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.ReportComment;

public class ReportCommentControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact(DisplayName = "When a comment is reported, the trimmed reason is submitted and Ok is returned.")]
    public async Task Report_Valid_ReturnsOk()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _plugin.Setup(p => p.ReportCommentAsync(commentId, _userId, "spam")).ReturnsAsync(true);
        var controller = new ReportCommentController(_plugin.Object).WithUser(_userId);

        // Act
        var result = await controller.ReportComment(commentId, new ReportCommentRequest { Reason = "  spam " });

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the report cannot be submitted, the controller returns BadRequest.")]
    public async Task Report_Failed_ReturnsBadRequest()
    {
        // Arrange
        var controller = new ReportCommentController(_plugin.Object).WithUser(_userId);

        // Act
        var result = await controller.ReportComment(Guid.NewGuid(), new ReportCommentRequest { Reason = "spam" });

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
