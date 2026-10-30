namespace PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;

public class ModerationAuditResponseDto
{
    public List<ModerationAuditLogItemDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ModerationAuditLogItemDto
{
    public string Id { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string PerformedByUserId { get; set; } = string.Empty;
    public ModerationAuditUserInfoDto PerformedByUser { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? PreviousValues { get; set; }
    public string? NewValues { get; set; }
    public string? ModerationNotes { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class ModerationAuditUserInfoDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GetModerationAuditLogsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Action { get; set; }
    public string? UserId { get; set; }
    public string? ModelId { get; set; }
}
