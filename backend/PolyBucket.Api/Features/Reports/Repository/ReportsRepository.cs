using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.Repository
{
    public class ReportsRepository(PolyBucketDbContext context) : IReportsRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<Report> CreateAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId)
        {
            return await Task.FromResult(_context.Reports
                .Where(r => r.Type == type && r.TargetId == targetId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList());
        }

        public async Task<IEnumerable<Report>> GetUnresolvedReportsAsync()
        {
            return await Task.FromResult(_context.Reports
                .Where(r => !r.IsResolved)
                .OrderByDescending(r => r.CreatedAt)
                .ToList());
        }

        public async Task<Report?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_context.Reports.Find(id));
        }

        public async Task UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }
    }
} 