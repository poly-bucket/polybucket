using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Http;
using PolyBucket.Api.Features.SystemSettings.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetTokenSettings;

public class GetTokenSettingsControllerTests
{
    [Fact(DisplayName = "When token settings load, GetTokenSettings returns Ok.")]
    public async Task GetTokenSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new TokenSettings { AccessTokenExpiryHours = 2 };
        var service = new Mock<IGetTokenSettingsService>();
        service.Setup(s => s.GetTokenSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        var controller = new GetTokenSettingsController(service.Object);

        // Act
        var result = await controller.GetTokenSettings(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(settings);
    }
}
