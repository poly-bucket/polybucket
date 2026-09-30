using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.Repository;

namespace PolyBucket.Api.Features.Email.RetryEmailMessage.Domain;

public class RetryEmailMessageService(
    IEmailOutboxRepository repository,
    IEmailDispatchSignal signal,
    TimeProvider timeProvider,
    ILogger<RetryEmailMessageService> logger) : IRetryEmailMessageService
{
    public async Task RetryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Email message {id} was not found.");

        if (message.Status is not (EmailMessageStatus.Failed or EmailMessageStatus.DeadLetter))
        {
            throw new ConflictException($"Only failed or dead-lettered emails can be retried; this one is {message.Status}.");
        }

        if (!await repository.RequeueAsync(id, timeProvider.GetUtcNow().UtcDateTime, cancellationToken))
        {
            throw new ConflictException("The email changed state while it was being retried.");
        }

        signal.Notify();
        logger.LogInformation("Email {MessageId} was requeued by an administrator", id);
    }
}
