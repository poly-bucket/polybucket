namespace PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Http;
public class ModeratorReportActivityResponseDto { public List<ModeratorReportActivityItemDto> Activity { get; set; } = new(); }
public class ModeratorReportActivityItemDto {
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public int ReportsResolved { get; set; }
    public int ReportsDismissed { get; set; }
    public int ReportsArchived { get; set; }
    public DateTime LastActivity { get; set; }
    public double AverageResolutionTime { get; set; }
}
