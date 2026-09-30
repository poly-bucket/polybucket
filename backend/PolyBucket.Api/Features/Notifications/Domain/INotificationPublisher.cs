using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Notifications.Domain;

public interface INotificationPublisher
{
    Task<bool> PublishAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
