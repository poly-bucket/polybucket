using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Repository;

public class GetUnreadNotificationCountRepository(PolyBucketDbContext context) : IGetUnreadNotificationCountRepository
{
    public Task<int> CountUnreadAsync(Guid userId, DateTime now, CancellationToken cancellationToken) =>
        context.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt == null && (n.ExpiresAt == null || n.ExpiresAt > now) && !n.IsRead)
            .CountAsync(cancellationToken);
}
