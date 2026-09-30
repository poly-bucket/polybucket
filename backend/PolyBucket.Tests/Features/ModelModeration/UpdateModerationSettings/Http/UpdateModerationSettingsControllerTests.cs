using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Domain;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration.UpdateModerationSettings.Http;

public class UpdateModerationSettingsControllerTests
{
    private readonly Mock<IUpdateModerationSettingsService> _service = new();

    [Fact(DisplayName = "Updating moderation settings returns the saved DTO.")]
    public async Task UpdateModerationSettings_Valid_ReturnsOk()
    {
        // Arrange
        var input = new ModerationSettingsDto { RequireModeration = false, AutoApproveVerifiedUsers = true };
        var updated = new ModerationSettingsDto { RequireModeration = false, AutoApproveVerifiedUsers = true };
        _service.Setup(s => s.UpdateAsync(input, It.IsAny<CancellationToken>())).ReturnsAsync(updated);
        var controller = new UpdateModerationSettingsController(_service.Object);

        // Act
        var result = await controller.UpdateModerationSettings(input, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(updated);
    }
}
