using PolyBucket.Email.RetryEmailMessage;
using Inner = PolyBucket.Email.RetryEmailMessage.IRetryEmailMessageService;

namespace PolyBucket.Api.Features.Email.RetryEmailMessage.Repository;

public class RetryEmailMessageRepository(Inner inner) : IRetryEmailMessageRepository
{
    public Task RetryAsync(Guid id, CancellationToken cancellationToken = default) =>
        inner.RetryAsync(id, cancellationToken);
}
