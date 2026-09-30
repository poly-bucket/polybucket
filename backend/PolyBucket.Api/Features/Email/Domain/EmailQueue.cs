using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Repository;

namespace PolyBucket.Api.Features.Email.Domain;

public class EmailQueue(
    IEmailOutboxRepository repository,
    IEmailSettingsResolver settingsResolver,
    IEmailDispatchSignal signal,
    ILogger<EmailQueue> logger) : IEmailQueue
{
    public async Task<EmailEnqueueOutcome> EnqueueAsync(EmailRequest request, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        if (!EmailAddressValidator.IsValid(request.Recipient))
        {
            throw new ArgumentException("Recipient must be a valid email address.", nameof(request));
        }

        if (!EmailTemplateCatalog.Templates.TryGetValue(request.Template, out var template))
        {
            throw new ArgumentException($"Unknown email template '{request.Template}'.", nameof(request));
        }

        var missing = template.RequiredFields
            .Where(field => !request.Model.TryGetValue(field, out var value) || string.IsNullOrWhiteSpace(value))
            .ToList();
        if (missing.Count > 0)
        {
            throw new ArgumentException($"Email template '{request.Template}' is missing required fields: {string.Join(", ", missing)}.", nameof(request));
        }

        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        if (!settings.IsEnabled)
        {
            logger.LogWarning("Email delivery is disabled; {Template} email was not queued", request.Template);
            return EmailEnqueueOutcome.EmailDisabled;
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey)
            && await repository.ExistsByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken))
        {
            return EmailEnqueueOutcome.Duplicate;
        }

        var message = new EmailMessage
        {
            TemplateKey = request.Template,
            Recipient = request.Recipient.Trim(),
            ModelJson = JsonSerializer.Serialize(request.Model),
            IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey,
            Status = EmailMessageStatus.Pending,
            NextAttemptAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        repository.Add(message);

        if (saveChanges)
        {
            await repository.SaveChangesAsync(cancellationToken);
        }

        signal.Notify();
        logger.LogInformation("Queued {Template} email {MessageId}", request.Template, message.Id);
        return EmailEnqueueOutcome.Queued;
    }
}
