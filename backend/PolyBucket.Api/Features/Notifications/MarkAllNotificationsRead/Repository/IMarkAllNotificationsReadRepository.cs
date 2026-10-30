namespace PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Repository;

public interface IMarkAllNotificationsReadRepository
{
    Task<int> MarkAllReadAsync(Guid userId, DateTime now, CancellationToken cancellationToken);
}
