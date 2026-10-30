using PolyBucket.Api.Features.ThemeManagement.CreateTheme.Http;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme.Repository;
using PolyBucket.Database.Entities.Themes;

namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme.Domain;

public interface ICreateThemeService
{
    Task<CreateThemeResponse> CreateThemeAsync(CreateThemeRequest request, CancellationToken cancellationToken = default);
}

public class CreateThemeService(ICreateThemeRepository repository) : ICreateThemeService
{
    public async Task<CreateThemeResponse> CreateThemeAsync(CreateThemeRequest request, CancellationToken cancellationToken = default)
    {
        if (await repository.ThemeNameExistsAsync(request.Name, cancellationToken))
            return new CreateThemeResponse { Success = false, Message = "A theme with this name already exists." };

        var existing = await repository.GetAllThemesAsync(cancellationToken);
        var isFirst = existing.Count == 0;
        var theme = new Theme
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = isFirst || request.IsDefault,
            IsActive = isFirst,
            Colors = new ThemeColors
            {
                Primary = request.Colors.Primary,
                PrimaryLight = request.Colors.PrimaryLight,
                PrimaryDark = request.Colors.PrimaryDark,
                Secondary = request.Colors.Secondary,
                SecondaryLight = request.Colors.SecondaryLight,
                SecondaryDark = request.Colors.SecondaryDark,
                Accent = request.Colors.Accent,
                AccentLight = request.Colors.AccentLight,
                AccentDark = request.Colors.AccentDark,
                BackgroundPrimary = request.Colors.BackgroundPrimary,
                BackgroundSecondary = request.Colors.BackgroundSecondary,
                BackgroundTertiary = request.Colors.BackgroundTertiary
            }
        };
        var created = await repository.CreateThemeAsync(theme, cancellationToken);
        if (request.IsDefault && !isFirst)
            await repository.SetThemeAsDefaultAsync(created.Id, cancellationToken);

        return new CreateThemeResponse
        {
            Success = true,
            Message = "Theme created successfully.",
            Theme = Map(created)
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
