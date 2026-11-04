namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

public class GetFileSettingsQueryParams
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SearchTerm { get; set; }

    public string? Category { get; set; }
}
