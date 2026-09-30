using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.GetEmailSettings.Domain;
using PolyBucket.Api.Features.Email.GetEmailSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class GetEmailSettingsControllerTests
{
    private readonly Mock<IGetEmailSettingsService> _service = new();
    private readonly GetEmailSettingsController _controller;

    public GetEmailSettingsControllerTests()
    {
        _controller = new GetEmailSettingsController(_service.Object);
    }

    [Fact(DisplayName = "When settings are requested, the controller returns 200 with the effective settings.")]
    public async Task GetEmailSettings_ReturnsOk()
    {
        // Arrange
        var dto = new EmailSettingsDto { Enabled = true, Transport = EmailTransportKind.Smtp, SmtpHost = "smtp.example.com" };
        _service.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetEmailSettings();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(dto);
        _service.Verify(s => s.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the service fails, GetEmailSettings propagates the exception.")]
    public async Task GetEmailSettings_ServiceThrows_Propagates()
    {
        // Arrange
        _service.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new System.InvalidOperationException("db"));

        // Act & Assert
        await Should.ThrowAsync<System.InvalidOperationException>(() => _controller.GetEmailSettings());
    }
}
