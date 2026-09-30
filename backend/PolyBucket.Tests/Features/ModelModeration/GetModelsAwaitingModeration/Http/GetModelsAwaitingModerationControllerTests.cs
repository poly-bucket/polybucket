using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration.GetModelsAwaitingModeration.Http;

public class GetModelsAwaitingModerationControllerTests
{
    private readonly Mock<IGetModelsAwaitingModerationService> _service = new();

    [Fact(DisplayName = "Valid pagination returns the moderation queue.")]
    public async Task GetModelsAwaitingModeration_Valid_ReturnsOk()
    {
        // Arrange
        var response = new ModelsAwaitingModerationResponse { Page = 1, PageSize = 20, TotalCount = 0 };
        _service.Setup(s => s.GetAsync(1, 20, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new GetModelsAwaitingModerationController(_service.Object);

        // Act
        var result = await controller.GetModelsAwaitingModeration(1, 20, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Theory(DisplayName = "Invalid pagination parameters return BadRequest.")]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetModelsAwaitingModeration_InvalidPaging_ReturnsBadRequest(int page, int pageSize)
    {
        // Arrange
        var controller = new GetModelsAwaitingModerationController(_service.Object);

        // Act
        var result = await controller.GetModelsAwaitingModeration(page, pageSize, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _service.Verify(s => s.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
