using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Notifications.Domain;

namespace PolyBucket.Api.Features.Notifications.Repository;

public class NotificationRepository(PolyBucketDbContext context) : INotificationRepository
{
    public void Add(Notification notification)
    {
        context.Notifications.Add(notification);
    }

    public async Task<bool> ExistsByDedupeKeyAsync(Guid userId, string dedupeKey, CancellationToken cancellationToken)
    {
        if (context.Notifications.Local.Any(n => n.UserId == userId && n.DedupeKey == dedupeKey))
        {
            return true;
        }

        return await context.Notifications.AnyAsync(n => n.UserId == userId && n.DedupeKey == dedupeKey, cancellationToken);
    }

    public Task<NotificationRecipient?> GetRecipientAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => new NotificationRecipient(
                u.Id,
                u.Username,
                u.Email,
                u.Settings == null || u.Settings.EmailNotifications,
                u.Settings == null || u.Settings.NotifyOnLikes,
                u.Settings == null || u.Settings.NotifyOnComments,
                u.Settings == null || u.Settings.NotifyOnFollows))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetUsernameAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPageAsync(Guid userId, bool unreadOnly, int page, int pageSize, DateTime now, CancellationToken cancellationToken)
    {
        var query = Visible(userId, now);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenBy(n => n.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<int> CountUnreadAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        return Visible(userId, now).CountAsync(n => !n.IsRead, cancellationToken);
    }

    public async Task<bool> MarkReadAsync(Guid userId, Guid notificationId, DateTime now, CancellationToken cancellationToken)
    {
        var updated = await context.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId && n.DeletedAt == null && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), cancellationToken);

        if (updated > 0)
        {
            return true;
        }

        return await context.Notifications.AnyAsync(n => n.Id == notificationId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
    }

    public Task<int> MarkAllReadAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        return context.Notifications
            .Where(n => n.UserId == userId && n.DeletedAt == null && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), cancellationToken);
    }

    private IQueryable<Notification> Visible(Guid userId, DateTime now)
    {
        return context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt == null && (n.ExpiresAt == null || n.ExpiresAt > now));
    }
}
