namespace PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Domain;

public class BulkUpdateFileSettingsCommand
{
    public List<Guid> Ids { get; set; } = new();

    public bool Enabled { get; set; }
}

public class BulkUpdateFileSettingsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UpdatedCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkUpdateFileSettingsItemResult> Results { get; set; } = new();
}

public class BulkUpdateFileSettingsItemResult
{
    public Guid Id { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
