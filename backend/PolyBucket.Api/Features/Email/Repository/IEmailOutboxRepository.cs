using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Email.Domain;

namespace PolyBucket.Api.Features.Email.Repository;

public interface IEmailOutboxRepository
{
    void Add(EmailMessage message);
    Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailMessage>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, DateTime now, CancellationToken cancellationToken = default);
    Task MarkSentAsync(Guid id, DateTime sentAt, bool scrubModel, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(Guid id, string error, DateTime nextAttemptAt, CancellationToken cancellationToken = default);
    Task MarkDeadLetterAsync(Guid id, string error, CancellationToken cancellationToken = default);
    Task<int> DeleteSentBeforeAsync(DateTime cutoff, CancellationToken cancellationToken = default);
    Task<long> CountPendingAsync(CancellationToken cancellationToken = default);
    Task<EmailMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<EmailMessage> Items, int TotalCount)> GetPageAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> RequeueAsync(Guid id, DateTime now, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<EmailMessageStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
}
