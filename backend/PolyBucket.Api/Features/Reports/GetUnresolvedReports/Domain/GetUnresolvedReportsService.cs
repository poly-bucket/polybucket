using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetUnresolvedReports.Repository;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetUnresolvedReports.Domain
{
    public class GetUnresolvedReportsService(IGetUnresolvedReportsRepository repository) : IGetUnresolvedReportsService
    {
        private readonly IGetUnresolvedReportsRepository _repository = repository;

        public Task<IEnumerable<Report>> GetUnresolvedReportsAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetUnresolvedReportsAsync(cancellationToken);
        }
    }
}
