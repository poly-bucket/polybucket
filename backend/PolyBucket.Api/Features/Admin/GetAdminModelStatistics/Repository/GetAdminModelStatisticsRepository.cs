using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Repository;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Repository
{
    public class GetAdminModelStatisticsRepository : IGetAdminModelStatisticsRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public GetAdminModelStatisticsRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ModelStatisticsData> GetModelStatisticsAsync(CancellationToken cancellationToken)
        {
            // Get basic model counts
            var totalModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null, cancellationToken);
            var publicModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.Privacy == PrivacySettings.Public, cancellationToken);
            var privateModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.Privacy == PrivacySettings.Private, cancellationToken);
            var unlistedModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.Privacy == PrivacySettings.Unlisted, cancellationToken);
            var aiGeneratedModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.AIGenerated, cancellationToken);
            var workInProgressModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.WIP, cancellationToken);
            var nsfwModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.NSFW, cancellationToken);
            var remixModels = await _dbContext.Models.CountAsync(m => m.DeletedAt == null && m.IsRemix, cancellationToken);

            // Get file statistics
            var fileStats = await _dbContext.ModelFiles
                .Where(f => f.DeletedAt == null)
                .GroupBy(f => 1)
                .Select(g => new
                {
                    TotalFiles = g.Count(),
                    TotalFileSizeBytes = g.Sum(f => f.Size)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var totalFiles = fileStats?.TotalFiles ?? 0;
            var totalFileSizeBytes = fileStats?.TotalFileSizeBytes ?? 0;

            // Get last activity dates
            var lastModelUploaded = await _dbContext.Models
                .Where(m => m.DeletedAt == null)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var lastModelUpdated = await _dbContext.Models
                .Where(m => m.DeletedAt == null)
                .OrderByDescending(m => m.UpdatedAt)
                .Select(m => m.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            // Get top uploaders
            var topUploadersData = await _dbContext.Models
                .Where(m => m.DeletedAt == null)
                .Select(m => new { m.AuthorId, m.Author.Username, m.Files })
                .ToArrayAsync(cancellationToken);

            var topUploaders = topUploadersData
                .GroupBy(m => new { m.AuthorId, m.Username })
                .Select(g => new TopUploaderData
                {
                    UserId = g.Key.AuthorId,
                    Username = g.Key.Username ?? "Unknown",
                    ModelCount = g.Count(),
                    TotalFileSizeBytes = g.SelectMany(m => m.Files.Where(f => f.DeletedAt == null)).Sum(f => f.Size)
                })
                .OrderByDescending(u => u.ModelCount)
                .Take(10)
                .ToArray();

            // Get file type distribution
            var fileTypeDistribution = await _dbContext.ModelFiles
                .Where(f => f.DeletedAt == null)
                .Select(f => new { f.Name, f.Size })
                .ToArrayAsync(cancellationToken);

            var fileTypeGroups = fileTypeDistribution
                .GroupBy(f => System.IO.Path.GetExtension(f.Name).ToLowerInvariant())
                .Select(g => new FileTypeStatsData
                {
                    FileExtension = g.Key,
                    Count = g.Count(),
                    TotalSizeBytes = g.Sum(f => f.Size),
                    Percentage = 0 // Will be calculated in service
                })
                .OrderByDescending(f => f.Count)
                .ToArray();

            // Calculate percentages for file types
            if (totalFiles > 0)
            {
                foreach (var fileType in fileTypeGroups)
                {
                    fileType.Percentage = (double)fileType.Count / totalFiles * 100;
                }
            }

            // Note: Pending review and flagged models would need additional logic
            // based on your moderation system. For now, setting to 0.
            var pendingReviewModels = 0;
            var flaggedModels = 0;

            return new ModelStatisticsData
            {
                TotalModels = totalModels,
                TotalFileSizeBytes = totalFileSizeBytes,
                TotalFiles = totalFiles,
                PublicModels = publicModels,
                PrivateModels = privateModels,
                UnlistedModels = unlistedModels,
                PendingReviewModels = pendingReviewModels,
                FlaggedModels = flaggedModels,
                AIGeneratedModels = aiGeneratedModels,
                WorkInProgressModels = workInProgressModels,
                NSFWModels = nsfwModels,
                RemixModels = remixModels,
                LastModelUploaded = lastModelUploaded,
                LastModelUpdated = lastModelUpdated,
                TopUploaders = topUploaders,
                FileTypeDistribution = fileTypeGroups
            };
        }
    }
}
