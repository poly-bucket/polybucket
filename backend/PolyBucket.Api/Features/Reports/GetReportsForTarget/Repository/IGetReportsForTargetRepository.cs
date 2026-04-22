using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReportsForTarget.Repository
{
    public interface IGetReportsForTargetRepository
    {
        Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId, CancellationToken cancellationToken = default);
    }
}
