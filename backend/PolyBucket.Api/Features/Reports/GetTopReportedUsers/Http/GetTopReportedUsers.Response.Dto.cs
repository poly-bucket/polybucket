namespace PolyBucket.Api.Features.Reports.GetTopReportedUsers.Http;

public class TopReportedUsersResponseDto
{
    public List<TopReportedUserItemDto> Items { get; set; } = new();
}

public class TopReportedUserItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime LastReported { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
}
