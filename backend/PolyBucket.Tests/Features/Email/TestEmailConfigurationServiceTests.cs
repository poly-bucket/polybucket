using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

public class TestEmailConfigurationServiceTests
{
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly Mock<IEmailTransportFactory> _factory = new();
    private readonly Mock<IEmailTransport> _transport = new();
    private readonly Mock<IEmailBrandingProvider> _branding = new();
    private readonly Mock<ITestEmailConfigurationRepository> _repository = new();

    private TestEmailConfigurationService CreateService(EffectiveEmailSettings settings)
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _factory.Setup(f => f.Get(It.IsAny<EmailTransportKind>())).Returns(_transport.Object);
        _branding.Setup(b => b.GetBrandingAsync(It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailBranding("PolyBucket", string.Empty));
        return new TestEmailConfigurationService(
            _resolver.Object,
            _factory.Object,
            new EmailTemplateRenderer(),
            _branding.Object,
            _repository.Object,
            TimeProvider.System,
            NullLogger<TestEmailConfigurationService>.Instance);
    }

    private static EffectiveEmailSettings Enabled => new() { Transport = EmailTransportKind.Smtp, SmtpHost = "smtp.example.com", FromAddress = "noreply@example.com" };

    [Fact(DisplayName = "When email is disabled, the test fails at the configuration stage without touching the transport.")]
    public async Task Test_EmailDisabled_FailsAtConfiguration()
    {
        // Arrange
        var service = CreateService(new EffectiveEmailSettings());

        // Act
        var result = await service.TestAsync("admin@example.com");

        // Assert
        result.Success.ShouldBeFalse();
        result.Stages.ShouldHaveSingleItem().Stage.ShouldBe(EmailDiagnosticStage.Configuration);
        _transport.Verify(t => t.DiagnoseAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the diagnostic succeeds, the successful test time is recorded and the settings cache is cleared.")]
    public async Task Test_Success_RecordsTimestamp()
    {
        // Arrange
        var service = CreateService(Enabled);
        _transport.Setup(t => t.DiagnoseAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailDiagnosticResult(true, [new EmailDiagnosticStageResult(EmailDiagnosticStage.Send, true, "ok", 5)]));

        // Act
        var result = await service.TestAsync("admin@example.com");

        // Assert
        result.Success.ShouldBeTrue();
        _repository.Verify(r => r.RecordSuccessfulTestAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _resolver.Verify(r => r.Invalidate(), Times.Once);
    }

    [Fact(DisplayName = "When the diagnostic fails, the message comes from the failing stage and no success is recorded.")]
    public async Task Test_Failure_ReportsFailingStage()
    {
        // Arrange
        var service = CreateService(Enabled);
        _transport.Setup(t => t.DiagnoseAsync(It.IsAny<EmailEnvelope>(), It.IsAny<EffectiveEmailSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailDiagnosticResult(false,
            [
                new EmailDiagnosticStageResult(EmailDiagnosticStage.Connect, true, "Connected", 3),
                new EmailDiagnosticStageResult(EmailDiagnosticStage.Auth, false, "Authentication failed: 535", 4)
            ]));

        // Act
        var result = await service.TestAsync("admin@example.com");

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Authentication failed: 535");
        _repository.Verify(r => r.RecordSuccessfulTestAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
