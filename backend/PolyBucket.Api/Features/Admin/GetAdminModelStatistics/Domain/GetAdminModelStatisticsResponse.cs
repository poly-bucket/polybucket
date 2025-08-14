using System;

namespace PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain
{
    public class GetAdminModelStatisticsResponse
    {
        public int TotalModels { get; set; }
        public long TotalFileSizeBytes { get; set; }
        public string TotalFileSizeFormatted { get; set; } = string.Empty;
        public int TotalFiles { get; set; }
        public int PublicModels { get; set; }
        public int PrivateModels { get; set; }
        public int UnlistedModels { get; set; }
        public int PendingReviewModels { get; set; }
        public int FlaggedModels { get; set; }
        public int AIGeneratedModels { get; set; }
        public int WorkInProgressModels { get; set; }
        public int NSFWModels { get; set; }
        public int RemixModels { get; set; }
        public DateTime? LastModelUploaded { get; set; }
        public DateTime? LastModelUpdated { get; set; }
        public double AverageFileSizeMB { get; set; }
        public double AverageFilesPerModel { get; set; }
        public TopUploader[] TopUploaders { get; set; } = Array.Empty<TopUploader>();
        public FileTypeStats[] FileTypeDistribution { get; set; } = Array.Empty<FileTypeStats>();
    }

    public class TopUploader
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int ModelCount { get; set; }
        public long TotalFileSizeBytes { get; set; }
        public string TotalFileSizeFormatted { get; set; } = string.Empty;
    }

    public class FileTypeStats
    {
        public string FileExtension { get; set; } = string.Empty;
        public int Count { get; set; }
        public long TotalSizeBytes { get; set; }
        public string TotalSizeFormatted { get; set; } = string.Empty;
        public double Percentage { get; set; }
    }
}
