using PolyBucket.Email.GetEmailOutbox;
using Inner = PolyBucket.Email.GetEmailOutbox.IGetEmailOutboxService;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Repository;

public class GetEmailOutboxRepository(Inner inner) : IGetEmailOutboxRepository
{
    public Task<EmailOutboxPageDto> GetAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default) =>
        inner.GetAsync(status, page, pageSize, cancellationToken);
}
