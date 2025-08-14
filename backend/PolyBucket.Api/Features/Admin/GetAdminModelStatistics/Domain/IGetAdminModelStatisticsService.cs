using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain
{
    public interface IGetAdminModelStatisticsService
    {
        Task<GetAdminModelStatisticsResponse> GetAdminModelStatisticsAsync(CancellationToken cancellationToken);
    }
}
