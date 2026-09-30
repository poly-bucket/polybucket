using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Email.Domain;

public class EmailTransportHealthCheck(IEmailSettingsResolver settingsResolver, PolyBucketDbContext context) : IHealthCheck
{
    private static readonly TimeSpan StaleQueueThreshold = TimeSpan.FromMinutes(15);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthContext, CancellationToken cancellationToken = default)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        var data = new Dictionary<string, object> { ["transport"] = settings.Transport.ToString() };

        if (!settings.IsEnabled)
        {
            return settings.RequireEmailVerification
                ? HealthCheckResult.Degraded("Email verification is required but email delivery is disabled.", data: data)
                : HealthCheckResult.Healthy("Email delivery is disabled.", data);
        }

        var errors = settings.GetValidationErrors();
        if (errors.Count > 0)
        {
            data["errors"] = errors;
            return HealthCheckResult.Degraded("Email configuration is invalid.", data: data);
        }

        var staleBefore = DateTime.UtcNow - StaleQueueThreshold;
        var oldestPending = await context.EmailMessages
            .AsNoTracking()
            .Where(m => m.Status == EmailMessageStatus.Pending || m.Status == EmailMessageStatus.Failed)
            .OrderBy(m => m.CreatedAt)
            .Select(m => (DateTime?)m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var deadLetters = await context.EmailMessages
            .AsNoTracking()
            .CountAsync(m => m.Status == EmailMessageStatus.DeadLetter, cancellationToken);

        data["deadLetters"] = deadLetters;
        if (oldestPending.HasValue)
        {
            data["oldestPendingAt"] = oldestPending.Value;
        }

        if (oldestPending < staleBefore)
        {
            return HealthCheckResult.Degraded("Emails have been waiting for delivery for more than 15 minutes.", data: data);
        }

        return HealthCheckResult.Healthy("Email delivery is configured.", data);
    }
}
