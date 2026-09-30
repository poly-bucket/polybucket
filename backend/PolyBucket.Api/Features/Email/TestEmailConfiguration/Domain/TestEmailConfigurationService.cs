using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;

public class TestEmailConfigurationService(
    IEmailSettingsResolver settingsResolver,
    IEmailTransportFactory transportFactory,
    IEmailTemplateRenderer renderer,
    IEmailBrandingProvider brandingProvider,
    ITestEmailConfigurationRepository repository,
    TimeProvider timeProvider,
    ILogger<TestEmailConfigurationService> logger) : ITestEmailConfigurationService
{
    public async Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        if (!settings.IsEnabled)
        {
            return Failed(EmailDiagnosticStage.Configuration, "Email delivery is disabled. Choose a transport and save before testing.");
        }

        var transport = transportFactory.Get(settings.Transport);
        if (transport == null)
        {
            return Failed(EmailDiagnosticStage.Configuration, $"No transport is available for '{settings.Transport}'.");
        }

        var branding = await brandingProvider.GetBrandingAsync(settings, cancellationToken);
        var rendered = renderer.Render(EmailTemplateKey.Test, new Dictionary<string, string>(), branding);
        var envelope = new EmailEnvelope
        {
            To = recipient.Trim(),
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TextBody = rendered.TextBody,
            Headers = new Dictionary<string, string> { ["X-PolyBucket-Template"] = nameof(EmailTemplateKey.Test) }
        };

        var diagnostic = await transport.DiagnoseAsync(envelope, settings, cancellationToken);
        if (!diagnostic.Success)
        {
            var failedStage = diagnostic.Stages.LastOrDefault(s => !s.Success);
            logger.LogWarning("Email configuration test failed at stage {Stage}", failedStage?.Stage);
            return new EmailTestResult(false, failedStage?.Message ?? "The test email could not be sent.", diagnostic.Stages);
        }

        await repository.RecordSuccessfulTestAsync(timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        settingsResolver.Invalidate();
        logger.LogInformation("Email configuration test succeeded using transport {Transport}", settings.Transport);

        return new EmailTestResult(true, $"Test email sent to {envelope.To}. Check the inbox (and spam folder).", diagnostic.Stages);
    }

    private static EmailTestResult Failed(EmailDiagnosticStage stage, string message)
    {
        return new EmailTestResult(false, message, [new EmailDiagnosticStageResult(stage, false, message, 0)]);
    }
}
