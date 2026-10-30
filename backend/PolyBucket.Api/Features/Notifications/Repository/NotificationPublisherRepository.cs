using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Notifications.Repository;

public class NotificationPublisherRepository(PolyBucketDbContext context) : INotificationPublisherRepository
{
    public void Add(Notification notification) => context.Notifications.Add(notification);

    public async Task<bool> ExistsByDedupeKeyAsync(Guid userId, string dedupeKey, CancellationToken cancellationToken)
    {
        if (context.Notifications.Local.Any(n => n.UserId == userId && n.DedupeKey == dedupeKey)) return true;
        return await context.Notifications.AnyAsync(n => n.UserId == userId && n.DedupeKey == dedupeKey, cancellationToken);
    }

    public Task<NotificationRecipient?> GetRecipientAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Users.AsNoTracking().Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => new NotificationRecipient(u.Id, u.Username, u.Email,
                u.Settings == null || u.Settings.EmailNotifications,
                u.Settings == null || u.Settings.NotifyOnLikes,
                u.Settings == null || u.Settings.NotifyOnComments,
                u.Settings == null || u.Settings.NotifyOnFollows))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<string?> GetUsernameAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.Username).FirstOrDefaultAsync(cancellationToken);
}
