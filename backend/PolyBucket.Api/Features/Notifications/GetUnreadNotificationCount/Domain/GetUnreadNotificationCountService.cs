using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Notifications.Repository;

namespace PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Domain;

public class GetUnreadNotificationCountService(INotificationRepository repository, TimeProvider timeProvider) : IGetUnreadNotificationCountService
{
    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.CountUnreadAsync(userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}
