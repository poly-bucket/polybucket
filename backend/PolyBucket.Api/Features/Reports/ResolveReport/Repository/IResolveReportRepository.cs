using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.ResolveReport.Repository
{
    public interface IResolveReportRepository
    {
        Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Report report, CancellationToken cancellationToken = default);
    }
}
