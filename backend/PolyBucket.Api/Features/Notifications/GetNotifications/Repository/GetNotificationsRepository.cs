using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Notifications.GetNotifications.Repository;

public class GetNotificationsRepository(PolyBucketDbContext context) : IGetNotificationsRepository
{
    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPageAsync(Guid userId, bool unreadOnly, int page, int pageSize, DateTime now, CancellationToken cancellationToken)
    {
        var query = Visible(userId, now);
        if (unreadOnly) query = query.Where(n => !n.IsRead);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(n => n.CreatedAt).ThenBy(n => n.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<int> CountUnreadAsync(Guid userId, DateTime now, CancellationToken cancellationToken) =>
        Visible(userId, now).CountAsync(n => !n.IsRead, cancellationToken);

    private IQueryable<Notification> Visible(Guid userId, DateTime now) =>
        context.Notifications.AsNoTracking().Where(n => n.UserId == userId && n.DeletedAt == null && (n.ExpiresAt == null || n.ExpiresAt > now));
}
