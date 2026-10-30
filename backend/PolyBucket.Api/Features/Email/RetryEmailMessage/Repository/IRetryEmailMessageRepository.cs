namespace PolyBucket.Api.Features.Email.RetryEmailMessage.Repository;

public interface IRetryEmailMessageRepository
{
    Task RetryAsync(Guid id, CancellationToken cancellationToken = default);
}
