using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReport.Domain
{
    public interface IGetReportService
    {
        Task<Report?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default);
    }
}
