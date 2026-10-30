using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateAuthenticationSettings;

public class UpdateAuthenticationSettingsControllerTests
{
    [Fact(DisplayName = "When settings are invalid, UpdateAuthenticationSettings returns BadRequest.")]
    public async Task UpdateAuthenticationSettings_Invalid_ReturnsBadRequest()
    {
        // Arrange
        var service = new Mock<IUpdateAuthenticationSettingsService>();
        var controller = new UpdateAuthenticationSettingsController(service.Object);
        var invalid = new AuthenticationSettings { AllowEmailLogin = false, AllowUsernameLogin = false };

        // Act
        var result = await controller.UpdateAuthenticationSettings(invalid, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        service.Verify(s => s.UpdateAuthenticationSettingsAsync(It.IsAny<AuthenticationSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When update succeeds, UpdateAuthenticationSettings returns Ok.")]
    public async Task UpdateAuthenticationSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new AuthenticationSettings { AllowEmailLogin = true, AllowUsernameLogin = true, LoginMethod = LoginMethod.Both };
        var service = new Mock<IUpdateAuthenticationSettingsService>();
        service.Setup(s => s.UpdateAuthenticationSettingsAsync(settings, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var controller = new UpdateAuthenticationSettingsController(service.Object);

        // Act
        var result = await controller.UpdateAuthenticationSettings(settings, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
