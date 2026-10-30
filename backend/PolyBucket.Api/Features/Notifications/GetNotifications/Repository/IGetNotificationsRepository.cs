using PolyBucket.Api.Features.Notifications.Domain;

namespace PolyBucket.Api.Features.Notifications.GetNotifications.Repository;

public interface IGetNotificationsRepository
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPageAsync(Guid userId, bool unreadOnly, int page, int pageSize, DateTime now, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid userId, DateTime now, CancellationToken cancellationToken);
}
