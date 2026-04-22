using PolyBucket.Api.Features.Reports.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetAllReports.Domain
{
    public interface IGetAllReportsService
    {
        Task<ReportsResponse> GetAllReportsAsync(int page, int pageSize, bool? isResolved, ReportType? type, CancellationToken cancellationToken = default);
    }
}
