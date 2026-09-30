using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class UpdateEmailSettingsControllerTests
{
    private readonly Mock<IUpdateEmailSettingsService> _service = new();
    private readonly UpdateEmailSettingsController _controller;

    public UpdateEmailSettingsControllerTests()
    {
        _controller = new UpdateEmailSettingsController(_service.Object);
    }

    private static UpdateEmailSettingsRequest Request() => new()
    {
        Transport = EmailTransportKind.Smtp,
        SmtpHost = "smtp.example.com",
        SmtpPort = 587,
        SmtpPassword = "secret",
        FromAddress = "noreply@example.com",
        PublicBaseUrl = "https://models.example.com"
    };

    [Fact(DisplayName = "When the update succeeds, the controller maps the request to the service and returns 200.")]
    public async Task UpdateEmailSettings_Valid_ReturnsOk()
    {
        // Arrange
        EmailSettingsUpdate? captured = null;
        var dto = new EmailSettingsDto { Enabled = true };
        _service.Setup(s => s.UpdateAsync(It.IsAny<EmailSettingsUpdate>(), It.IsAny<CancellationToken>()))
            .Callback<EmailSettingsUpdate, CancellationToken>((u, _) => captured = u)
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.UpdateEmailSettings(Request());

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(dto);
        captured.ShouldNotBeNull();
        captured!.SmtpHost.ShouldBe("smtp.example.com");
        captured.SmtpPassword.ShouldBe("secret");
        captured.Transport.ShouldBe(EmailTransportKind.Smtp);
    }

    [Fact(DisplayName = "When the service reports validation errors, the controller returns 400 with every error.")]
    public async Task UpdateEmailSettings_ValidationErrors_ReturnsBadRequest()
    {
        // Arrange
        _service.Setup(s => s.UpdateAsync(It.IsAny<EmailSettingsUpdate>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainValidationException(["SMTP host is required.", "A valid From address is required."]));

        // Act
        var result = await _controller.UpdateEmailSettings(Request());

        // Assert
        var badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        var problem = badRequest.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Errors[string.Empty].Length.ShouldBe(2);
    }

    [Fact(DisplayName = "When an environment-managed field is changed, the controller returns 409.")]
    public async Task UpdateEmailSettings_EnvironmentConflict_ReturnsConflict()
    {
        // Arrange
        _service.Setup(s => s.UpdateAsync(It.IsAny<EmailSettingsUpdate>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("smtpHost is managed by environment variables."));

        // Act
        var result = await _controller.UpdateEmailSettings(Request());

        // Assert
        var conflict = result.Result.ShouldBeOfType<ConflictObjectResult>();
        conflict.Value.ShouldBeOfType<ProblemDetails>().Detail.ShouldContain("smtpHost");
    }

    [Fact(DisplayName = "When model binding fails, the controller returns 400 without calling the service.")]
    public async Task UpdateEmailSettings_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError(nameof(UpdateEmailSettingsRequest.SmtpPort), "Out of range");

        // Act
        var result = await _controller.UpdateEmailSettings(Request());

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _service.Verify(s => s.UpdateAsync(It.IsAny<EmailSettingsUpdate>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
