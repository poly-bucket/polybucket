using PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Repository;

namespace PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Domain;

public class GetUnreadNotificationCountService(IGetUnreadNotificationCountRepository repository, TimeProvider timeProvider) : IGetUnreadNotificationCountService
{
    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken) =>
        repository.CountUnreadAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
}
