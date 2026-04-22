using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Domain
{
    public interface ISubmitReportService
    {
        Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description, CancellationToken cancellationToken = default);
    }
}
