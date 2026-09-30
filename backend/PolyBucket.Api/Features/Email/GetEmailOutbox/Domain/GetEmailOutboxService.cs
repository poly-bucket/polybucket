using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.Repository;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;

public class GetEmailOutboxService(IEmailOutboxRepository repository) : IGetEmailOutboxService
{
    public const int MaxPageSize = 100;

    public async Task<EmailOutboxPageDto> GetAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, total) = await repository.GetPageAsync(status, page, pageSize, cancellationToken);
        var counts = await repository.GetStatusCountsAsync(cancellationToken);

        return new EmailOutboxPageDto
        {
            Items = [.. items.Select(m => new EmailOutboxItemDto
            {
                Id = m.Id,
                Template = m.TemplateKey,
                Recipient = m.Recipient,
                Status = m.Status,
                Attempts = m.Attempts,
                NextAttemptAt = m.NextAttemptAt,
                LastError = m.LastError,
                CreatedAt = m.CreatedAt,
                SentAt = m.SentAt
            })],
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            StatusCounts = counts.ToDictionary(pair => pair.Key, pair => pair.Value)
        };
    }
}
