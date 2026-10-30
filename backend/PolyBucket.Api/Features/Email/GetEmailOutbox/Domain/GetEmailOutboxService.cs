using PolyBucket.Email.GetEmailOutbox;
using PolyBucket.Api.Features.Email.GetEmailOutbox.Repository;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;

public class GetEmailOutboxService(IGetEmailOutboxRepository repository) : IGetEmailOutboxService
{
    public Task<EmailOutboxPageDto> GetAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default) =>
        repository.GetAsync(status, page, pageSize, cancellationToken);
}
