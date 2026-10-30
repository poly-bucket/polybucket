namespace PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;

public class TopReportedModelsResponseDto
{
    public List<TopReportedModelItemDto> Items { get; set; } = new();
}

public class TopReportedModelItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime LastReported { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
}
