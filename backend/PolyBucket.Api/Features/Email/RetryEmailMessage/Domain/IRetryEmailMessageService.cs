using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Email.RetryEmailMessage.Domain;

public interface IRetryEmailMessageService
{
    Task RetryAsync(Guid id, CancellationToken cancellationToken = default);
}
