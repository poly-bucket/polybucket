using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Repository
{
    public class SubmitReportRepository(PolyBucketDbContext context) : ISubmitReportRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<Report> CreateAsync(Report report, CancellationToken cancellationToken = default)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync(cancellationToken);
            return report;
        }
    }
}
