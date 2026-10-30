using PolyBucket.Api.Features.ThemeManagement.GetThemes.Http;
using PolyBucket.Api.Features.ThemeManagement.GetThemes.Repository;
using PolyBucket.Database.Entities.Themes;

namespace PolyBucket.Api.Features.ThemeManagement.GetThemes.Domain;

public interface IGetThemesService
{
    Task<ThemeListResponse> GetThemesAsync(CancellationToken cancellationToken = default);
}

public class GetThemesService(IGetThemesRepository repository) : IGetThemesService
{
    public async Task<ThemeListResponse> GetThemesAsync(CancellationToken cancellationToken = default)
    {
        var themes = await repository.GetAllThemesAsync(cancellationToken);
        var active = await repository.GetActiveThemeAsync(cancellationToken);
        return new ThemeListResponse
        {
            Themes = themes.Select(Map).ToList(),
            ActiveTheme = active == null ? null : Map(active)
        };
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
