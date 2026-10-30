using PolyBucket.Api.Features.Notifications.MarkNotificationRead.Repository;

namespace PolyBucket.Api.Features.Notifications.MarkNotificationRead.Domain;

public class MarkNotificationReadService(IMarkNotificationReadRepository repository, TimeProvider timeProvider) : IMarkNotificationReadService
{
    public Task<bool> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken) =>
        repository.MarkReadAsync(userId, notificationId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
}
