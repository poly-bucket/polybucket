using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.SubmitReport.Repository;
using PolyBucket.Api.Features.SystemSettings.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Domain
{
    public class SubmitReportService(ISubmitReportRepository repository, ISiteModelAdvancedPolicy advancedPolicy) : ISubmitReportService
    {
        private readonly ISubmitReportRepository _repository = repository;
        private readonly ISiteModelAdvancedPolicy _advancedPolicy = advancedPolicy;

        public async Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description, CancellationToken cancellationToken = default)
        {
            var advanced = await _advancedPolicy.GetSnapshotAsync(cancellationToken);
            if (!advanced.EnableModelReporting)
            {
                throw new UnauthorizedAccessException("Reporting is disabled on this site.");
            }

            if (advanced.RequireReportDetails && string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Report details are required.");
            }

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
