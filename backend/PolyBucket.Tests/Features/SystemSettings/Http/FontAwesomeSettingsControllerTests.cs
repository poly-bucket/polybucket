using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.SystemSettings.Http;

public class FontAwesomeSettingsControllerTests
{
    private readonly Mock<IFontAwesomeSettingsService> _service = new();

    [Fact(DisplayName = "When FontAwesome settings load, GetSettings returns Ok.")]
    public async Task GetSettings_Success_ReturnsOk()
    {
        // Arrange
        var settings = new FontAwesomeSettings { IsProEnabled = false };
        _service.Setup(s => s.GetSettingsAsync()).ReturnsAsync(settings);
        var controller = new FontAwesomeSettingsController(_service.Object, NullLogger<FontAwesomeSettingsController>.Instance);

        // Act
        var result = await controller.GetSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(settings);
    }

    [Fact(DisplayName = "When settings body is null, UpdateSettings returns BadRequest.")]
    public async Task UpdateSettings_NullBody_ReturnsBadRequest()
    {
        // Arrange
        var controller = new FontAwesomeSettingsController(_service.Object, NullLogger<FontAwesomeSettingsController>.Instance);

        // Act
        var result = await controller.UpdateSettings(null!);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When license key is empty, TestLicense returns BadRequest.")]
    public async Task TestLicense_EmptyKey_ReturnsBadRequest()
    {
        // Arrange
        var controller = new FontAwesomeSettingsController(_service.Object, NullLogger<FontAwesomeSettingsController>.Instance);

        // Act
        var result = await controller.TestLicense(new TestLicenseRequest { LicenseKey = " " });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
