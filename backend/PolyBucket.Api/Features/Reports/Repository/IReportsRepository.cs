using PolyBucket.Api.Features.Reports.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.Repository
{
    public interface IReportsRepository
    {
        Task<Report> CreateAsync(Report report);
        Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId);
        Task<IEnumerable<Report>> GetUnresolvedReportsAsync();
        Task<Report?> GetByIdAsync(Guid id);
        Task UpdateAsync(Report report);
    }
} 