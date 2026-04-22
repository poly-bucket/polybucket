using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.GetAllReports.Repository
{
    public class GetAllReportsRepository(PolyBucketDbContext context) : IGetAllReportsRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public Task<ReportsResponse> GetPagedAsync(int page, int pageSize, bool? isResolved, ReportType? type, CancellationToken cancellationToken = default)
        {
            var query = _context.Reports.AsQueryable();

            if (isResolved.HasValue)
            {
                query = query.Where(r => r.IsResolved == isResolved.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var reports = query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new ReportsResponse
            {
                Reports = reports,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
    }
}
