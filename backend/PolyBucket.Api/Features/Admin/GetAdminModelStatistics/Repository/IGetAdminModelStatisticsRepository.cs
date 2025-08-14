using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Repository
{
    public interface IGetAdminModelStatisticsRepository
    {
        Task<ModelStatisticsData> GetModelStatisticsAsync(CancellationToken cancellationToken);
    }

    public class ModelStatisticsData
    {
        public int TotalModels { get; set; }
        public long TotalFileSizeBytes { get; set; }
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
        public TopUploaderData[] TopUploaders { get; set; } = Array.Empty<TopUploaderData>();
        public FileTypeStatsData[] FileTypeDistribution { get; set; } = Array.Empty<FileTypeStatsData>();
    }

    public class TopUploaderData
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int ModelCount { get; set; }
        public long TotalFileSizeBytes { get; set; }
    }

    public class FileTypeStatsData
    {
        public string FileExtension { get; set; } = string.Empty;
        public int Count { get; set; }
        public long TotalSizeBytes { get; set; }
        public double Percentage { get; set; }
    }
}
