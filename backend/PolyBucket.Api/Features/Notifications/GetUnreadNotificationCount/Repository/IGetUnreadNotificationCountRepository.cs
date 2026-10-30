namespace PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Repository;

public interface IGetUnreadNotificationCountRepository
{
    Task<int> CountUnreadAsync(Guid userId, DateTime now, CancellationToken cancellationToken);
}
