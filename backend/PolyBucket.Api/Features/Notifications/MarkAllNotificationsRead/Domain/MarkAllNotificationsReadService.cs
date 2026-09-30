using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Notifications.Repository;

namespace PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Domain;

public class MarkAllNotificationsReadService(INotificationRepository repository, TimeProvider timeProvider) : IMarkAllNotificationsReadService
{
    public Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.MarkAllReadAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}
