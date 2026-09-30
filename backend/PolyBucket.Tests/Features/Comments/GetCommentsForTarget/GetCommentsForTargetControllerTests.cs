using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.GetCommentsForTarget;

public class GetCommentsForTargetControllerTests
{
    private readonly Mock<IEnhancedCommentsPlugin> _plugin = new();
    private readonly Mock<ICommentResponseMapper> _mapper = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _modelId = Guid.NewGuid();

    public GetCommentsForTargetControllerTests()
    {
        _plugin.Setup(p => p.GetCommentsForTargetAsync(It.IsAny<CommentTarget>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<EnhancedComment>());
        _plugin.Setup(p => p.GetCommentCountForTargetAsync(It.IsAny<CommentTarget>(), It.IsAny<bool>())).ReturnsAsync(45);
        _plugin.Setup(p => p.GetCommentStatisticsAsync(It.IsAny<CommentTarget>())).ReturnsAsync(new CommentStatistics());
        _mapper.Setup(m => m.MapAsync(It.IsAny<IReadOnlyList<EnhancedComment>>(), It.IsAny<Guid?>(), It.IsAny<bool>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommentResponse>());
    }

    [Fact(DisplayName = "When listing model comments, the page and total pages are returned.")]
    public async Task Get_ValidTarget_ReturnsPage()
    {
        // Arrange
        var controller = new GetCommentsForTargetController(_plugin.Object, _mapper.Object).WithUser(_userId);

        // Act
        var result = await controller.GetCommentsForTarget("model", _modelId, page: 2, pageSize: 20);

        // Assert
        var page = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<CommentsPagedResponse>();
        page.TotalCount.ShouldBe(45);
        page.TotalPages.ShouldBe(3);
        page.Page.ShouldBe(2);
    }

    [Fact(DisplayName = "When a regular user asks for hidden comments, hidden comments are still excluded.")]
    public async Task Get_IncludeHiddenAsUser_IsIgnored()
    {
        // Arrange
        var controller = new GetCommentsForTargetController(_plugin.Object, _mapper.Object).WithUser(_userId);

        // Act
        await controller.GetCommentsForTarget("model", _modelId, includeHidden: true);

        // Assert
        _plugin.Verify(p => p.GetCommentsForTargetAsync(It.IsAny<CommentTarget>(), false, 1, 20), Times.Once);
    }

    [Fact(DisplayName = "When a moderator asks for hidden comments, they are included.")]
    public async Task Get_IncludeHiddenAsModerator_IsHonored()
    {
        // Arrange
        var controller = new GetCommentsForTargetController(_plugin.Object, _mapper.Object).WithUser(_userId, "Moderator");

        // Act
        await controller.GetCommentsForTarget("model", _modelId, includeHidden: true);

        // Assert
        _plugin.Verify(p => p.GetCommentsForTargetAsync(It.IsAny<CommentTarget>(), true, 1, 20), Times.Once);
    }

    [Theory(DisplayName = "When the target type or paging is invalid, the controller returns BadRequest.")]
    [InlineData("printer", 1, 20)]
    [InlineData("model", 0, 20)]
    [InlineData("model", 1, 500)]
    public async Task Get_InvalidInput_ReturnsBadRequest(string targetType, int page, int pageSize)
    {
        // Arrange
        var controller = new GetCommentsForTargetController(_plugin.Object, _mapper.Object).WithUser(_userId);

        // Act
        var result = await controller.GetCommentsForTarget(targetType, _modelId, page, pageSize);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
