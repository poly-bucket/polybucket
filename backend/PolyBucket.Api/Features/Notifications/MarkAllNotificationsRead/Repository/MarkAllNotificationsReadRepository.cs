using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Repository;

public class MarkAllNotificationsReadRepository(PolyBucketDbContext context) : IMarkAllNotificationsReadRepository
{
    public Task<int> MarkAllReadAsync(Guid userId, DateTime now, CancellationToken cancellationToken) =>
        context.Notifications
            .Where(n => n.UserId == userId && n.DeletedAt == null && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, now), cancellationToken);
}
