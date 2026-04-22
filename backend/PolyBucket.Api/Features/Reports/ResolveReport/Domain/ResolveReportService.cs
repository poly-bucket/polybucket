using PolyBucket.Api.Features.Reports.ResolveReport.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.ResolveReport.Domain
{
    public class ResolveReportService(IResolveReportRepository repository) : IResolveReportService
    {
        private readonly IResolveReportRepository _repository = repository;

        public async Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdAsync(reportId, cancellationToken);
            if (report != null)
            {
                report.IsResolved = true;
                report.Resolution = resolution;
                report.ResolvedAt = DateTime.UtcNow;
                report.ResolvedById = resolverId;
                await _repository.UpdateAsync(report, cancellationToken);
            }
        }
    }
}
