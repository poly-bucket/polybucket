using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Notifications.MarkNotificationRead.Domain;

public interface IMarkNotificationReadService
{
    Task<bool> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);
}
