using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Common.Email;

public class LogEmailTransport(ILogger<LogEmailTransport> logger) : IEmailTransport
{
    public EmailTransportKind Kind => EmailTransportKind.Log;

    public Task SendAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email transport is set to Log. Message {MessageId} to {Recipient} with subject {Subject} was not delivered.",
            envelope.MessageId,
            envelope.To,
            envelope.Subject);
        return Task.CompletedTask;
    }

    public Task<EmailDiagnosticResult> DiagnoseAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        var errors = settings.GetValidationErrors();
        var stages = errors.Count > 0
            ? new[] { new EmailDiagnosticStageResult(EmailDiagnosticStage.Configuration, false, string.Join(" ", errors), 0) }
            : new[]
            {
                new EmailDiagnosticStageResult(EmailDiagnosticStage.Configuration, true, "Configuration is valid.", 0),
                new EmailDiagnosticStageResult(EmailDiagnosticStage.Send, true, "Log transport records messages in the application log instead of delivering them.", 0)
            };

        return Task.FromResult(new EmailDiagnosticResult(errors.Count == 0, stages));
    }
}
