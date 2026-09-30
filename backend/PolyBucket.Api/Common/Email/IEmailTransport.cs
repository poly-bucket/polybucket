using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Common.Email;

public sealed record EmailEnvelope
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string TextBody { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}

public sealed record EmailDiagnosticStageResult(EmailDiagnosticStage Stage, bool Success, string Message, long ElapsedMilliseconds);

public sealed record EmailDiagnosticResult(bool Success, IReadOnlyList<EmailDiagnosticStageResult> Stages);

public interface IEmailTransport
{
    EmailTransportKind Kind { get; }

    Task SendAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default);

    Task<EmailDiagnosticResult> DiagnoseAsync(EmailEnvelope envelope, EffectiveEmailSettings settings, CancellationToken cancellationToken = default);
}

public interface IEmailTransportFactory
{
    IEmailTransport? Get(EmailTransportKind kind);
}

public class EmailTransportFactory(IEnumerable<IEmailTransport> transports) : IEmailTransportFactory
{
    private readonly IReadOnlyList<IEmailTransport> _transports = [.. transports];

    public IEmailTransport? Get(EmailTransportKind kind)
    {
        for (var i = _transports.Count - 1; i >= 0; i--)
        {
            if (_transports[i].Kind == kind)
            {
                return _transports[i];
            }
        }

        return null;
    }
}

public class EmailDeliveryException(string message, bool isTransient, Exception? inner = null) : Exception(message, inner)
{
    public bool IsTransient { get; } = isTransient;
}
