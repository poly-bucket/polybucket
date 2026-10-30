using PolyBucket.Api.Features.Email.RetryEmailMessage.Repository;

namespace PolyBucket.Api.Features.Email.RetryEmailMessage.Domain;

public class RetryEmailMessageService(IRetryEmailMessageRepository repository) : IRetryEmailMessageService
{
    public Task RetryAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.RetryAsync(id, cancellationToken);
}
