using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportsForTarget.Repository;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReportsForTarget.Domain
{
    public class GetReportsForTargetService(IGetReportsForTargetRepository repository) : IGetReportsForTargetService
    {
        private readonly IGetReportsForTargetRepository _repository = repository;

        public Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId, CancellationToken cancellationToken = default)
        {
            return _repository.GetReportsForTargetAsync(type, targetId, cancellationToken);
        }
    }
}
