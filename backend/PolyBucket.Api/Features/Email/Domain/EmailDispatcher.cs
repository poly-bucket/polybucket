using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Repository;

namespace PolyBucket.Api.Features.Email.Domain;

public interface IEmailDispatcher
{
    Task<int> DispatchPendingAsync(CancellationToken cancellationToken = default);
    Task<int> PurgeSentAsync(CancellationToken cancellationToken = default);
}

public class EmailDispatcher(
    IEmailOutboxRepository repository,
    IEmailSettingsResolver settingsResolver,
    IEmailTransportFactory transportFactory,
    IEmailTemplateRenderer renderer,
    IEmailBrandingProvider brandingProvider,
    IOptions<EmailOptions> options,
    TimeProvider timeProvider,
    ILogger<EmailDispatcher> logger) : IEmailDispatcher
{
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(6);

    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken = default)
    {
        var dispatcherOptions = options.Value.Dispatcher;
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var transport = settings.CanDeliver ? transportFactory.Get(settings.Transport) : null;

        if (transport == null)
        {
            EmailMetrics.SetQueueDepth(await repository.CountPendingAsync(cancellationToken));
            return 0;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var batch = await repository.ClaimBatchAsync(
            dispatcherOptions.BatchSize,
            TimeSpan.FromSeconds(dispatcherOptions.LockSeconds),
            now,
            cancellationToken);

        if (batch.Count > 0)
        {
            var branding = await brandingProvider.GetBrandingAsync(settings, cancellationToken);
            foreach (var message in batch)
            {
                await DeliverAsync(message, transport, settings, branding, dispatcherOptions.MaxAttempts, cancellationToken);
            }
        }

        EmailMetrics.SetQueueDepth(await repository.CountPendingAsync(cancellationToken));
        return batch.Count;
    }

    public Task<int> PurgeSentAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = timeProvider.GetUtcNow().UtcDateTime.AddDays(-options.Value.Dispatcher.SentRetentionDays);
        return repository.DeleteSentBeforeAsync(cutoff, cancellationToken);
    }

    internal static TimeSpan ComputeRetryDelay(int attempts, double jitter)
    {
        var exponent = Math.Clamp(attempts - 1, 0, 20);
        var delay = BaseRetryDelay.TotalSeconds * Math.Pow(2, exponent);
        delay = Math.Min(delay, MaxRetryDelay.TotalSeconds);
        var factor = 1 + Math.Clamp(jitter, -1, 1) * 0.2;
        return TimeSpan.FromSeconds(delay * factor);
    }

    internal static Dictionary<string, string> BuildHeaders(EmailTemplateKey templateKey, string templateName, EffectiveEmailSettings settings)
    {
        var headers = new Dictionary<string, string>
        {
            ["X-PolyBucket-Template"] = templateName,
            ["Auto-Submitted"] = "auto-generated"
        };

        if (templateKey == EmailTemplateKey.Notification && settings.HasPublicBaseUrl)
        {
            headers["List-Unsubscribe"] = $"<{settings.BuildUrl("/settings/notifications")}>";
        }

        return headers;
    }

    private async Task DeliverAsync(
        EmailMessage message,
        IEmailTransport transport,
        EffectiveEmailSettings settings,
        EmailBranding branding,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        var templateName = message.TemplateKey.ToString();
        RenderedEmail rendered;
        try
        {
            var model = JsonSerializer.Deserialize<Dictionary<string, string>>(message.ModelJson) ?? [];
            rendered = renderer.Render(message.TemplateKey, model, branding);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            logger.LogError(ex, "Email {MessageId} could not be rendered and was dead-lettered", message.Id);
            await repository.MarkDeadLetterAsync(message.Id, $"Render failed: {ex.Message}", cancellationToken);
            EmailMetrics.RecordDeadLettered(templateName);
            return;
        }

        var envelope = new EmailEnvelope
        {
            MessageId = message.Id,
            To = message.Recipient,
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            TextBody = rendered.TextBody,
            Headers = BuildHeaders(message.TemplateKey, templateName, settings)
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await transport.SendAsync(envelope, settings, cancellationToken);
            var containsSecrets = EmailTemplateCatalog.Templates.TryGetValue(message.TemplateKey, out var definition) && definition.ContainsSecrets;
            await repository.MarkSentAsync(message.Id, timeProvider.GetUtcNow().UtcDateTime, containsSecrets, CancellationToken.None);
            EmailMetrics.RecordSent(templateName, stopwatch.Elapsed.TotalMilliseconds);
            logger.LogInformation("Sent {Template} email {MessageId} on attempt {Attempt}", templateName, message.Id, message.Attempts);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var isTransient = ex is not EmailDeliveryException deliveryException || deliveryException.IsTransient;
            if (!isTransient || message.Attempts >= maxAttempts)
            {
                logger.LogError(ex, "Email {MessageId} ({Template}) was dead-lettered after {Attempts} attempt(s)", message.Id, templateName, message.Attempts);
                await repository.MarkDeadLetterAsync(message.Id, ex.Message, CancellationToken.None);
                EmailMetrics.RecordDeadLettered(templateName);
                return;
            }

            var delay = ComputeRetryDelay(message.Attempts, Random.Shared.NextDouble() * 2 - 1);
            var nextAttempt = timeProvider.GetUtcNow().UtcDateTime.Add(delay);
            logger.LogWarning(ex, "Email {MessageId} ({Template}) failed on attempt {Attempts}; retrying at {NextAttempt}", message.Id, templateName, message.Attempts, nextAttempt);
            await repository.MarkFailedAsync(message.Id, ex.Message, nextAttempt, CancellationToken.None);
            EmailMetrics.RecordFailed(templateName);
        }
    }
}
