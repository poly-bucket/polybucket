using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Services
{
    public interface IUserStatisticsService
    {
        Task<UserStatistics> GetUserStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
    }

    public class UserStatistics
    {
        public Guid UserId { get; set; }
        public int TotalModels { get; set; }
        public int TotalCollections { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDownloads { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }
        public int TotalComments { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public long TotalStorageUsedBytes { get; set; }
    }
}
