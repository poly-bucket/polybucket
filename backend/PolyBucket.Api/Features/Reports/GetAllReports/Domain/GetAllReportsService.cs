using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetAllReports.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetAllReports.Domain
{
    public class GetAllReportsService(IGetAllReportsRepository repository) : IGetAllReportsService
    {
        private readonly IGetAllReportsRepository _repository = repository;

        public Task<ReportsResponse> GetAllReportsAsync(int page, int pageSize, bool? isResolved, ReportType? type, CancellationToken cancellationToken = default)
        {
            return _repository.GetPagedAsync(page, pageSize, isResolved, type, cancellationToken);
        }
    }
}
