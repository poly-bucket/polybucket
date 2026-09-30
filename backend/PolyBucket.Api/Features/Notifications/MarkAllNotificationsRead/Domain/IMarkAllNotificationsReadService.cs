using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Domain;

public interface IMarkAllNotificationsReadService
{
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken);
}
