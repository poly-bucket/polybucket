using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email.Templates;

namespace PolyBucket.Email.Domain;

public sealed record EmailRequest(
    EmailTemplateKey Template,
    string Recipient,
    IReadOnlyDictionary<string, string> Model,
    string? IdempotencyKey = null);

public enum EmailEnqueueOutcome
{
    Queued,
    Duplicate,
    EmailDisabled,
    MissingPublicBaseUrl
}

public interface IEmailQueue
{
    Task<EmailEnqueueOutcome> EnqueueAsync(EmailRequest request, bool saveChanges = true, CancellationToken cancellationToken = default);
}
