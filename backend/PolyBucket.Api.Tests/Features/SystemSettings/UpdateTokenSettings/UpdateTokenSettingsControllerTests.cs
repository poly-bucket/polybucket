using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateTokenSettings;

public class UpdateTokenSettingsControllerTests
{
    [Fact(DisplayName = "When update succeeds, UpdateTokenSettings returns Ok.")]
    public async Task UpdateTokenSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new TokenSettings { AccessTokenExpiryHours = 1 };
        var service = new Mock<IUpdateTokenSettingsService>();
        service.Setup(s => s.UpdateTokenSettingsAsync(settings, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new UpdateTokenSettingsController(service.Object);

        // Act
        var result = await controller.UpdateTokenSettings(settings, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
