using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReport.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReport.Domain
{
    public class GetReportService(IGetReportRepository repository) : IGetReportService
    {
        private readonly IGetReportRepository _repository = repository;

        public Task<Report?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
        {
            return _repository.GetByIdAsync(reportId, cancellationToken);
        }
    }
}
