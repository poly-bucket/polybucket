using PolyBucket.Email.GetEmailOutbox;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Repository;

public interface IGetEmailOutboxRepository
{
    Task<EmailOutboxPageDto> GetAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
}
