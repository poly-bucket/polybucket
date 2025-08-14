using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Services;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Api.Features.Comments.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Services
{
    public class UserStatisticsService : IUserStatisticsService
    {
        private readonly PolyBucketDbContext _dbContext;

        public UserStatisticsService(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserStatistics> GetUserStatisticsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var statistics = new UserStatistics { UserId = userId };

            // Get model statistics
            var modelStats = await _dbContext.Models
                .Where(m => m.AuthorId == userId && m.DeletedAt == null)
                .GroupBy(m => 1)
                .Select(g => new
                {
                    TotalModels = g.Count(),
                    TotalDownloads = g.Sum(m => m.Downloads),
                    TotalLikes = g.Sum(m => m.Likes),
                    LastActivityAt = g.Max(m => m.UpdatedAt)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (modelStats != null)
            {
                statistics.TotalModels = modelStats.TotalModels;
                statistics.TotalDownloads = modelStats.TotalDownloads;
                statistics.TotalLikes = modelStats.TotalLikes;
                statistics.LastActivityAt = modelStats.LastActivityAt;
            }

            // Get collection statistics
            var collectionCount = await _dbContext.Collections
                .CountAsync(c => c.OwnerId == userId && c.DeletedAt == null, cancellationToken);
            statistics.TotalCollections = collectionCount;

            // Get likes given by user (from Model.LikeCollection)
            var likesGiven = await _dbContext.Likes
                .Where(l => l.UserId == userId && l.DeletedAt == null)
                .CountAsync(cancellationToken);
            statistics.TotalLikes = likesGiven;

            // Get comment statistics using EnhancedComment
            var commentCount = await _dbContext.Comments
                .CountAsync(c => c.Author.Id == userId && c.DeletedAt == null, cancellationToken);
            statistics.TotalComments = commentCount;

            // Get storage usage from model files
            var storageUsed = await _dbContext.ModelFiles
                .Where(f => f.Model.AuthorId == userId && f.DeletedAt == null)
                .SumAsync(f => f.Size, cancellationToken);
            statistics.TotalStorageUsedBytes = storageUsed;

            // TODO: Implement followers/following when that feature is added
            statistics.TotalFollowers = 0;
            statistics.TotalFollowing = 0;

            return statistics;
        }
    }
}
