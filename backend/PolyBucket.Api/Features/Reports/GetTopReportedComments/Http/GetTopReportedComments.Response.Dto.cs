namespace PolyBucket.Api.Features.Reports.GetTopReportedComments.Http;

public class TopReportedCommentsResponseDto
{
    public List<TopReportedCommentItemDto> Items { get; set; } = new();
}

public class TopReportedCommentItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime LastReported { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
}
