using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Email.RetryEmailMessage;

public interface IRetryEmailMessageService
{
    Task RetryAsync(Guid id, CancellationToken cancellationToken = default);
}
