using PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Repository;

namespace PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Domain;

public class MarkAllNotificationsReadService(IMarkAllNotificationsReadRepository repository, TimeProvider timeProvider) : IMarkAllNotificationsReadService
{
    public Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken) =>
        repository.MarkAllReadAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
}
