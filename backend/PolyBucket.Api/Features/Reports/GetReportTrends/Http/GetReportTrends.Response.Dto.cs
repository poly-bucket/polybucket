namespace PolyBucket.Api.Features.Reports.GetReportTrends.Http;

public class ReportTrendsResponseDto
{
    public List<ReportTrendItemDto> Trends { get; set; } = new();
}

public class ReportTrendItemDto
{
    public DateTime Date { get; set; }
    public int TotalReports { get; set; }
    public int ResolvedReports { get; set; }
    public int ActiveReports { get; set; }
}
