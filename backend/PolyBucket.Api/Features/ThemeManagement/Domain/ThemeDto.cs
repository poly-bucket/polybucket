namespace PolyBucket.Api.Features.ThemeManagement.Domain;

public record ThemeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public ThemeColorsDto Colors { get; init; } = null!;
}

public record ThemeColorsDto
{
    public string Primary { get; init; } = null!;
    public string PrimaryLight { get; init; } = null!;
    public string PrimaryDark { get; init; } = null!;
    public string Secondary { get; init; } = null!;
    public string SecondaryLight { get; init; } = null!;
    public string SecondaryDark { get; init; } = null!;
    public string Accent { get; init; } = null!;
    public string AccentLight { get; init; } = null!;
    public string AccentDark { get; init; } = null!;
    public string BackgroundPrimary { get; init; } = null!;
    public string BackgroundSecondary { get; init; } = null!;
    public string BackgroundTertiary { get; init; } = null!;
}

public record CreateThemeRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsDefault { get; init; } = false;
    public ThemeColorsDto Colors { get; init; } = null!;
}

public record UpdateThemeRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsDefault { get; init; } = false;
    public ThemeColorsDto Colors { get; init; } = null!;
}

public record ThemeListResponse
{
    public List<ThemeDto> Themes { get; init; } = new();
    public ThemeDto? ActiveTheme { get; init; }
}

public record ThemeResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
    public ThemeDto? Theme { get; init; }
}
