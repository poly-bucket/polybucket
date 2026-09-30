using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Email;

namespace PolyBucket.Tests.Email;

public class CapturingEmailTransport : IEmailTransport
{
    private readonly ConcurrentQueue<EmailEnvelope> _sent = new();

    public EmailTransportKind Kind => EmailTransportKind.Log;

    public IReadOnlyList<EmailEnvelope> Sent => [.. _sent];

    public EmailDeliveryException? NextFailure { get; set; }

    public void Clear()
    {
        _sent.Clear();
        NextFailure = null;
    }

    public IReadOnlyList<EmailEnvelope> SentTo(string recipient) =>
        [.. _sent.Where(e => string.Equals(e.To, recipient, System.StringComparison.OrdinalIgnoreCase))];

    public Task SendAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        if (NextFailure is { } failure)
        {
            NextFailure = null;
            throw failure;
        }

        _sent.Enqueue(envelope);
        return Task.CompletedTask;
    }

    public Task<EmailDiagnosticResult> DiagnoseAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue(envelope);
        return Task.FromResult(new EmailDiagnosticResult(true,
        [
            new EmailDiagnosticStageResult(EmailDiagnosticStage.Configuration, true, "Configuration is valid.", 0),
            new EmailDiagnosticStageResult(EmailDiagnosticStage.Send, true, "Captured by test transport.", 0)
        ]));
    }
}
