using PolyBucket.Email.GetEmailOutbox;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;

public interface IGetEmailOutboxService
{
    Task<EmailOutboxPageDto> GetAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
}
