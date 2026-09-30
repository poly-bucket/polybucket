using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.Notifications.Repository;

namespace PolyBucket.Api.Features.Notifications.GetNotifications.Domain;

public class GetNotificationsService(INotificationRepository repository, TimeProvider timeProvider) : IGetNotificationsService
{
    public async Task<GetNotificationsResponse> GetNotificationsAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var (items, total) = await repository.GetPageAsync(userId, unreadOnly, page, pageSize, now, cancellationToken);
        var unread = unreadOnly ? total : await repository.CountUnreadAsync(userId, now, cancellationToken);

        return new GetNotificationsResponse
        {
            Items = items.Select(NotificationDto.From).ToList(),
            TotalCount = total,
            UnreadCount = unread,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }
}
