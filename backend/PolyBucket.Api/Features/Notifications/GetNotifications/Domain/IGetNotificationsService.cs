using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Notifications.GetNotifications.Domain;

public interface IGetNotificationsService
{
    Task<GetNotificationsResponse> GetNotificationsAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken);
}
