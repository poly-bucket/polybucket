using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration.GetModerationSettings.Http;

public class GetModerationSettingsControllerTests
{
    private readonly Mock<IGetModerationSettingsService> _service = new();

    [Fact(DisplayName = "Get moderation settings returns the current DTO.")]
    public async Task GetModerationSettings_ReturnsOk()
    {
        // Arrange
        var settings = new ModerationSettingsDto { RequireModeration = true, AutoApproveModels = false };
        _service.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        var controller = new GetModerationSettingsController(_service.Object);

        // Act
        var result = await controller.GetModerationSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(settings);
    }
}
