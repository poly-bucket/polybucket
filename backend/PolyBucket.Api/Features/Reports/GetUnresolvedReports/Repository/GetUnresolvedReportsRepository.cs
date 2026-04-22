using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetUnresolvedReports.Repository
{
    public class GetUnresolvedReportsRepository(PolyBucketDbContext context) : IGetUnresolvedReportsRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<IEnumerable<Report>> GetUnresolvedReportsAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Reports
                .Where(r => !r.IsResolved)
                .OrderByDescending(r => r.CreatedAt)
                .ToList());
        }
    }
}
