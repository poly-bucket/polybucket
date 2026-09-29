using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.LikeModel;

public class LikeModelControllerTests
{
    private readonly Mock<ILikeModelService> _mockService;
    private readonly Mock<ILogger<LikeModelController>> _mockLogger;
    private readonly LikeModelController _controller;

    public LikeModelControllerTests()
    {
        _mockService = new Mock<ILikeModelService>();
        _mockLogger = new Mock<ILogger<LikeModelController>>();
        _controller = new LikeModelController(_mockService.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "When liking a model with a valid request, the like model controller returns NoContent.")]
    public async Task LikeModel_WithValidRequest_ReturnsNoContent()
    {
        var modelId = Guid.NewGuid();
        _mockService
            .Setup(s => s.LikeModelAsync(modelId, It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.LikeModel(modelId, CancellationToken.None);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact(DisplayName = "When inspecting like model actions, ProducesResponseType attributes are applied.")]
    public void LikeModelActions_ShouldHaveProducesResponseTypeAttributes()
    {
        var likeMethod = typeof(LikeModelController).GetMethod(nameof(LikeModelController.LikeModel));
        likeMethod.ShouldNotBeNull();
        likeMethod!.GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Any(a => a.StatusCode == StatusCodes.Status204NoContent)
            .ShouldBeTrue();

        var unlikeMethod = typeof(LikeModelController).GetMethod(nameof(LikeModelController.UnlikeModel));
        unlikeMethod.ShouldNotBeNull();
        unlikeMethod!.GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Any(a => a.StatusCode == StatusCodes.Status204NoContent)
            .ShouldBeTrue();
    }
}
