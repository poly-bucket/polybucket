using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Email.Domain;

namespace PolyBucket.Api.Features.Email.Repository;

public class EmailOutboxRepository(PolyBucketDbContext context) : IEmailOutboxRepository
{
    private const int MaxErrorLength = 2000;

    public void Add(EmailMessage message)
    {
        context.EmailMessages.Add(message);
    }

    public Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return context.EmailMessages.AnyAsync(m => m.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailMessage>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, DateTime now, CancellationToken cancellationToken = default)
    {
        var lockedUntil = now.Add(lockDuration);
        var pending = nameof(EmailMessageStatus.Pending);
        var failed = nameof(EmailMessageStatus.Failed);
        var sending = nameof(EmailMessageStatus.Sending);

        return await context.EmailMessages
            .FromSqlInterpolated($@"
                UPDATE ""EmailMessages""
                SET ""Status"" = {sending}, ""LockedUntil"" = {lockedUntil}, ""Attempts"" = ""Attempts"" + 1
                WHERE ""Id"" IN (
                    SELECT ""Id"" FROM ""EmailMessages""
                    WHERE (""Status"" IN ({pending}, {failed}) AND ""NextAttemptAt"" <= {now})
                       OR (""Status"" = {sending} AND ""LockedUntil"" < {now})
                    ORDER BY ""NextAttemptAt""
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED)
                RETURNING *")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task MarkSentAsync(Guid id, DateTime sentAt, bool scrubModel, CancellationToken cancellationToken = default)
    {
        var query = context.EmailMessages.Where(m => m.Id == id);
        return scrubModel
            ? query.ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, EmailMessageStatus.Sent)
                .SetProperty(m => m.SentAt, sentAt)
                .SetProperty(m => m.LockedUntil, (DateTime?)null)
                .SetProperty(m => m.LastError, (string?)null)
                .SetProperty(m => m.ModelJson, "{}"), cancellationToken)
            : query.ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, EmailMessageStatus.Sent)
                .SetProperty(m => m.SentAt, sentAt)
                .SetProperty(m => m.LockedUntil, (DateTime?)null)
                .SetProperty(m => m.LastError, (string?)null), cancellationToken);
    }

    public Task MarkFailedAsync(Guid id, string error, DateTime nextAttemptAt, CancellationToken cancellationToken = default)
    {
        var truncated = Truncate(error);
        return context.EmailMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, EmailMessageStatus.Failed)
                .SetProperty(m => m.NextAttemptAt, nextAttemptAt)
                .SetProperty(m => m.LockedUntil, (DateTime?)null)
                .SetProperty(m => m.LastError, truncated), cancellationToken);
    }

    public Task MarkDeadLetterAsync(Guid id, string error, CancellationToken cancellationToken = default)
    {
        var truncated = Truncate(error);
        return context.EmailMessages
            .Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, EmailMessageStatus.DeadLetter)
                .SetProperty(m => m.LockedUntil, (DateTime?)null)
                .SetProperty(m => m.LastError, truncated), cancellationToken);
    }

    public Task<int> DeleteSentBeforeAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return context.EmailMessages
            .Where(m => m.Status == EmailMessageStatus.Sent && m.SentAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task<long> CountPendingAsync(CancellationToken cancellationToken = default)
    {
        return context.EmailMessages
            .LongCountAsync(m => m.Status == EmailMessageStatus.Pending
                || m.Status == EmailMessageStatus.Failed
                || m.Status == EmailMessageStatus.Sending, cancellationToken);
    }

    public Task<EmailMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.EmailMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<EmailMessage> Items, int TotalCount)> GetPageAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.EmailMessages.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> RequeueAsync(Guid id, DateTime now, CancellationToken cancellationToken = default)
    {
        var updated = await context.EmailMessages
            .Where(m => m.Id == id && (m.Status == EmailMessageStatus.Failed || m.Status == EmailMessageStatus.DeadLetter))
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, EmailMessageStatus.Pending)
                .SetProperty(m => m.Attempts, 0)
                .SetProperty(m => m.NextAttemptAt, now)
                .SetProperty(m => m.LockedUntil, (DateTime?)null), cancellationToken);

        return updated > 0;
    }

    public async Task<IReadOnlyDictionary<EmailMessageStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await context.EmailMessages
            .AsNoTracking()
            .GroupBy(m => m.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<EmailMessageStatus>()
            .ToDictionary(status => status, status => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0);
    }

    private static string Truncate(string value)
    {
        return value.Length <= MaxErrorLength ? value : value[..MaxErrorLength];
    }
}
