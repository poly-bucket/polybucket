namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Http;

public record SetActiveThemeResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}
