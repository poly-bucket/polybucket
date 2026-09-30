using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Notifications.Repository;

namespace PolyBucket.Api.Features.Notifications.MarkNotificationRead.Domain;

public class MarkNotificationReadService(INotificationRepository repository, TimeProvider timeProvider) : IMarkNotificationReadService
{
    public Task<bool> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        return repository.MarkReadAsync(userId, notificationId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}
