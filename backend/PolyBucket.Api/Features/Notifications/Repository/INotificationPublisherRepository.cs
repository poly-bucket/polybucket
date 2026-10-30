using PolyBucket.Api.Features.Notifications.Domain;

namespace PolyBucket.Api.Features.Notifications.Repository;

public interface INotificationPublisherRepository
{
    void Add(Notification notification);
    Task<bool> ExistsByDedupeKeyAsync(Guid userId, string dedupeKey, CancellationToken cancellationToken);
    Task<NotificationRecipient?> GetRecipientAsync(Guid userId, CancellationToken cancellationToken);
    Task<string?> GetUsernameAsync(Guid userId, CancellationToken cancellationToken);
}
