using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Notifications.MarkNotificationRead.Repository;

public class MarkNotificationReadRepository(PolyBucketDbContext context) : IMarkNotificationReadRepository
{
    public async Task<bool> MarkReadAsync(Guid userId, Guid notificationId, DateTime now, CancellationToken cancellationToken)
    {
        var updated = await context.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId && n.DeletedAt == null && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, now), cancellationToken);
        if (updated > 0) return true;
        return await context.Notifications.AnyAsync(n => n.Id == notificationId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
    }
}
