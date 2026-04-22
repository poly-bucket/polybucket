using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetReport.Repository
{
    public class GetReportRepository(PolyBucketDbContext context) : IGetReportRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_context.Reports.Find(new object[] { id }));
        }
    }
}
