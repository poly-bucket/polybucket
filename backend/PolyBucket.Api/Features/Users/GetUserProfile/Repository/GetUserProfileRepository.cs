using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain.Enums;

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

    public async Task<GetUserProfileStatisticsRow> GetProfileStatisticsAsync(Guid userId, bool includePrivate, CancellationToken cancellationToken = default)
    {
        var row = new GetUserProfileStatisticsRow();

        var modelsQuery = dbContext.Models
            .Where(m => m.AuthorId == userId && m.DeletedAt == null);

        if (!includePrivate)
        {
            modelsQuery = modelsQuery.Where(m => m.Privacy == PrivacySettings.Public);
        }

        var modelStats = await modelsQuery
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

        var collectionsQuery = dbContext.Collections
            .Where(c => c.OwnerId == userId && c.DeletedAt == null);
        if (!includePrivate)
        {
            collectionsQuery = collectionsQuery.Where(c => c.Visibility == CollectionVisibility.Public);
        }

        row.TotalCollections = await collectionsQuery.CountAsync(cancellationToken);

        var likesGiven = await dbContext.Likes
            .Where(l => l.UserId == userId && l.DeletedAt == null)
            .CountAsync(cancellationToken);
        row.TotalLikes = likesGiven;

        row.TotalFollowers = 0;
        row.TotalFollowing = 0;

        return row;
    }
}
