using PolyBucket.Api.Features.Reports.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetUnresolvedReports.Domain
{
    public interface IGetUnresolvedReportsService
    {
        Task<IEnumerable<Report>> GetUnresolvedReportsAsync(CancellationToken cancellationToken = default);
    }
}
