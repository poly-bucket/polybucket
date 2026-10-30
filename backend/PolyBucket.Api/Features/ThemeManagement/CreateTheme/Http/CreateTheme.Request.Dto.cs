namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme.Http;

public record CreateThemeRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public ThemeColorsDto Colors { get; init; } = null!;
}

public record CreateThemeResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public ThemeDto? Theme { get; init; }
}
