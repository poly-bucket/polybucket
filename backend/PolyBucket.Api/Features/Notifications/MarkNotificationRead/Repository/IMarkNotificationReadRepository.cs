namespace PolyBucket.Api.Features.Notifications.MarkNotificationRead.Repository;

public interface IMarkNotificationReadRepository
{
    Task<bool> MarkReadAsync(Guid userId, Guid notificationId, DateTime now, CancellationToken cancellationToken);
}
