using PolyBucket.Api.Features.Reports.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetAllReports.Repository
{
    public interface IGetAllReportsRepository
    {
        Task<ReportsResponse> GetPagedAsync(int page, int pageSize, bool? isResolved, ReportType? type, CancellationToken cancellationToken = default);
    }
}
