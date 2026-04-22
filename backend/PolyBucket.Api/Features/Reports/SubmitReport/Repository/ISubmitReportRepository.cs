using PolyBucket.Api.Features.Reports.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Repository
{
    public interface ISubmitReportRepository
    {
        Task<Report> CreateAsync(Report report, CancellationToken cancellationToken = default);
    }
}
