using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Notifications.Domain;

namespace PolyBucket.Api.Features.Notifications.Repository;

public interface INotificationRepository
{
    void Add(Notification notification);
    Task<bool> ExistsByDedupeKeyAsync(Guid userId, string dedupeKey, CancellationToken cancellationToken);
    Task<NotificationRecipient?> GetRecipientAsync(Guid userId, CancellationToken cancellationToken);
    Task<string?> GetUsernameAsync(Guid userId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPageAsync(Guid userId, bool unreadOnly, int page, int pageSize, DateTime now, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid userId, DateTime now, CancellationToken cancellationToken);
    Task<bool> MarkReadAsync(Guid userId, Guid notificationId, DateTime now, CancellationToken cancellationToken);
    Task<int> MarkAllReadAsync(Guid userId, DateTime now, CancellationToken cancellationToken);
}
