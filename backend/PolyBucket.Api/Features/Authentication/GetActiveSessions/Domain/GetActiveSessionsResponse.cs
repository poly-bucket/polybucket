namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Domain;

public class GetActiveSessionsResponse
{
    public IReadOnlyList<SessionSummaryDto> Sessions { get; set; } = [];
}

public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
}
