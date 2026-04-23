using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain;
using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Repository;
using PolyBucket.Api.Common.Models.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain
{
    public class GetAdminModelStatisticsService : IGetAdminModelStatisticsService
    {
        private readonly IGetAdminModelStatisticsRepository _repository;
        private readonly ILogger<GetAdminModelStatisticsService> _logger;

        public GetAdminModelStatisticsService(IGetAdminModelStatisticsRepository repository, ILogger<GetAdminModelStatisticsService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetAdminModelStatisticsResponse> GetAdminModelStatisticsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var statistics = await _repository.GetModelStatisticsAsync(cancellationToken);
                
                // Calculate additional statistics
                var response = new GetAdminModelStatisticsResponse
                {
                    TotalModels = statistics.TotalModels,
                    TotalFileSizeBytes = statistics.TotalFileSizeBytes,
                    TotalFileSizeFormatted = FormatFileSize(statistics.TotalFileSizeBytes),
                    TotalFiles = statistics.TotalFiles,
                    PublicModels = statistics.PublicModels,
                    PrivateModels = statistics.PrivateModels,
                    UnlistedModels = statistics.UnlistedModels,
                    PendingReviewModels = statistics.PendingReviewModels,
                    FlaggedModels = statistics.FlaggedModels,
                    AIGeneratedModels = statistics.AIGeneratedModels,
                    WorkInProgressModels = statistics.WorkInProgressModels,
                    NSFWModels = statistics.NSFWModels,
                    RemixModels = statistics.RemixModels,
                    LastModelUploaded = statistics.LastModelUploaded,
                    LastModelUpdated = statistics.LastModelUpdated,
                    AverageFileSizeMB = statistics.TotalFiles > 0 ? (double)statistics.TotalFileSizeBytes / (1024 * 1024) / statistics.TotalFiles : 0,
                    AverageFilesPerModel = statistics.TotalModels > 0 ? (double)statistics.TotalFiles / statistics.TotalModels : 0,
                    TopUploaders = statistics.TopUploaders.Select(u => new TopUploader
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        ModelCount = u.ModelCount,
                        TotalFileSizeBytes = u.TotalFileSizeBytes,
                        TotalFileSizeFormatted = FormatFileSize(u.TotalFileSizeBytes)
                    }).ToArray(),
                    FileTypeDistribution = statistics.FileTypeDistribution.Select(f => new FileTypeStats
                    {
                        FileExtension = f.FileExtension,
                        Count = f.Count,
                        TotalSizeBytes = f.TotalSizeBytes,
                        TotalSizeFormatted = FormatFileSize(f.TotalSizeBytes),
                        Percentage = f.Percentage
                    }).ToArray()
                };

                _logger.LogInformation("Admin model statistics retrieved successfully. Total models: {TotalModels}, Total file size: {TotalFileSize}", 
                    response.TotalModels, response.TotalFileSizeFormatted);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin model statistics");
                throw;
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            var unitIndex = 0;
            var size = (double)bytes;
            
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            
            return $"{size:F2} {units[unitIndex]}";
        }
    }
}
