using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Http;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Repository;
using PolyBucket.Database.Entities.Themes;

namespace PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Domain;

public interface IGetActiveThemeService
{
    Task<ThemeDto?> GetActiveThemeAsync(CancellationToken cancellationToken = default);
}

public class GetActiveThemeService(IGetActiveThemeRepository repository) : IGetActiveThemeService
{
    public async Task<ThemeDto?> GetActiveThemeAsync(CancellationToken cancellationToken = default)
    {
        var theme = await repository.GetActiveThemeAsync(cancellationToken);
        return theme == null ? null : Map(theme);
    }

    private static ThemeDto Map(Theme t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        IsDefault = t.IsDefault,
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        Colors = new ThemeColorsDto
        {
            Primary = t.Colors.Primary,
            PrimaryLight = t.Colors.PrimaryLight,
            PrimaryDark = t.Colors.PrimaryDark,
            Secondary = t.Colors.Secondary,
            SecondaryLight = t.Colors.SecondaryLight,
            SecondaryDark = t.Colors.SecondaryDark,
            Accent = t.Colors.Accent,
            AccentLight = t.Colors.AccentLight,
            AccentDark = t.Colors.AccentDark,
            BackgroundPrimary = t.Colors.BackgroundPrimary,
            BackgroundSecondary = t.Colors.BackgroundSecondary,
            BackgroundTertiary = t.Colors.BackgroundTertiary
        }
    };
}
