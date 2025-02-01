using Core.Models;
using Core.Plugins.Reports;
using Microsoft.EntityFrameworkCore;

namespace Database.Plugins;

public class DefaultReportingPlugin : IReportingPlugin
{
    private readonly Context _context;

    public DefaultReportingPlugin(Context context)
    {
        _context = context;
    }

    public string Id => "default-reporting-plugin";
    public string Name => "Default Reporting Plugin";
    public string Description => "Default implementation of the reporting plugin using Entity Framework Core";
    public string Version => "1.0.0";
    public string Author => "Polybucket Team";

    public Task InitializeAsync() => Task.CompletedTask;

    public Task UnloadAsync() => Task.CompletedTask;

    public async Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description)
    {
        var reporter = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == reporterId);
        if (reporter == null)
            throw new InvalidOperationException("Reporter not found");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            Type = type,
            TargetId = targetId,
            ReporterId = reporterId,
            Reason = reason,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId)
    {
        return await _context.Reports
            .Where(r => r.Type == type && r.TargetId == targetId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetUnresolvedReportsAsync()
    {
        return await _context.Reports
            .Where(r => !r.IsResolved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == reportId);
        if (report == null)
            throw new InvalidOperationException("Report not found");

        report.IsResolved = true;
        report.Resolution = resolution;
        report.ResolvedAt = DateTime.UtcNow;
        report.ResolvedById = resolverId;

        await _context.SaveChangesAsync();
    }

    public async Task<Report> GetReportByIdAsync(Guid reportId)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == reportId);
        if (report == null)
            throw new InvalidOperationException("Report not found");

        return report;
    }
}