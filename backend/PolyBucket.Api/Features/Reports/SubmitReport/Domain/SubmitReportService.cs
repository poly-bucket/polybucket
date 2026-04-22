using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.SubmitReport.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Domain
{
    public class SubmitReportService(ISubmitReportRepository repository) : ISubmitReportService
    {
        private readonly ISubmitReportRepository _repository = repository;

        public async Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description, CancellationToken cancellationToken = default)
        {
            var report = new Report
            {
                Type = type,
                TargetId = targetId,
                ReporterId = reporterId,
                Reason = reason,
                Description = description,
                IsResolved = false,
                Resolution = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateAsync(report, cancellationToken);
        }
    }
}
