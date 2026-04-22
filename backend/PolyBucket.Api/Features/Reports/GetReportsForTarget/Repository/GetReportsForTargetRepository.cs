using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReportsForTarget.Repository
{
    public class GetReportsForTargetRepository(PolyBucketDbContext context) : IGetReportsForTargetRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Reports
                .Where(r => r.Type == type && r.TargetId == targetId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList());
        }
    }
}
