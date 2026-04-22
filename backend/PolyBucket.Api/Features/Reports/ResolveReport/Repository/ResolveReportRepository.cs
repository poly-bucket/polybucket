using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.ResolveReport.Repository
{
    public class ResolveReportRepository(PolyBucketDbContext context) : IResolveReportRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Reports.Find(new object[] { id }));
        }

        public async Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
