using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.ResolveReport.Domain
{
    public interface IResolveReportService
    {
        Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution, CancellationToken cancellationToken = default);
    }
}
