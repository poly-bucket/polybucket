using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Users.GetUserProfile.Repository;

public class GetUserProfileRepository(PolyBucketDbContext dbContext) : IGetUserProfileRepository
{
    public Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<GetUserProfileStatisticsRow> GetProfileStatisticsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = new GetUserProfileStatisticsRow();

        var modelStats = await dbContext.Models
            .Where(m => m.AuthorId == userId && m.DeletedAt == null)
            .GroupBy(m => 1)
            .Select(g => new
            {
                TotalModels = g.Count(),
                TotalDownloads = g.Sum(m => m.Downloads),
                TotalLikes = g.Sum(m => m.Likes)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (modelStats != null)
        {
            row.TotalModels = modelStats.TotalModels;
            row.TotalDownloads = modelStats.TotalDownloads;
            row.TotalLikes = modelStats.TotalLikes;
        }

        row.TotalCollections = await dbContext.Collections
            .CountAsync(c => c.OwnerId == userId && c.DeletedAt == null, cancellationToken);

        var likesGiven = await dbContext.Likes
            .Where(l => l.UserId == userId && l.DeletedAt == null)
            .CountAsync(cancellationToken);
        row.TotalLikes = likesGiven;

        row.TotalFollowers = 0;
        row.TotalFollowing = 0;

        return row;
    }
}
