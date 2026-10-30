using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Domain;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Http;
using PolyBucket.Api.Features.Comments.Plugins;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetCommentsForTarget;

public class GetCommentsForTargetControllerTests
{
    private readonly Mock<IGetCommentsForTargetService> _service = new();
    private readonly Guid _modelId = Guid.NewGuid();

    [Fact(DisplayName = "When the target is valid, comments are returned.")]
    public async Task GetCommentsForTarget_Valid_ReturnsOk()
    {
        // Arrange
        _service.Setup(s => s.GetAsync("model", _modelId, 1, 20, false, false, It.IsAny<Guid?>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCommentsForTargetResult(new CommentsPagedResponse { TotalCount = 45 }, null));
        var controller = new GetCommentsForTargetController(_service.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetCommentsForTarget("model", _modelId, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "When the target type is invalid, the controller returns BadRequest.")]
    public async Task GetCommentsForTarget_InvalidTarget_ReturnsBadRequest()
    {
        // Arrange
        _service.Setup(s => s.GetAsync("printer", It.IsAny<Guid>(), 1, 20, false, false, It.IsAny<Guid?>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCommentsForTargetResult(null, "Invalid target type: printer"));
        var controller = new GetCommentsForTargetController(_service.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetCommentsForTarget("printer", _modelId, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
