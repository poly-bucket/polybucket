using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Domain;

public interface IGetUnreadNotificationCountService
{
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
}
