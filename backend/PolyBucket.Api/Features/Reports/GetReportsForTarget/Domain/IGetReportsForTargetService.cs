using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReportsForTarget.Domain
{
    public interface IGetReportsForTargetService
    {
        Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId, CancellationToken cancellationToken = default);
    }
}
