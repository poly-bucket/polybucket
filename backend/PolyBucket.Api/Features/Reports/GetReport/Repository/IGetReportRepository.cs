using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReport.Repository
{
    public interface IGetReportRepository
    {
        Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
