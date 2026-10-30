namespace PolyBucket.Api.Features.ThemeManagement.GetThemes.Http;

public record ThemeListResponse
{
    public List<ThemeDto> Themes { get; init; } = new();
    public ThemeDto? ActiveTheme { get; init; }
}
