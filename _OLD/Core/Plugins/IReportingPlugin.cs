using Core.Models;
using Core.Models.Enumerations;

namespace Core.Plugins;

public interface IReportingPlugin : IPlugin
{
    Task<Report> CreateReportAsync(ReportType type, string targetId, string reporterId, ReportReason reason, string? description);
    Task<Report> ResolveReportAsync(string reportId, string resolutionNotes);
    Task<IEnumerable<Report>> GetUnresolvedReportsAsync();
    Task<Report?> GetReportByIdAsync(string reportId);
} 