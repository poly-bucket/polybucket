using MediatR;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Features.ThemeManagement.Repository;

namespace PolyBucket.Api.Features.ThemeManagement.GetThemes;

public record GetThemesQuery : IRequest<ThemeListResponse>;

public class GetThemesQueryHandler : IRequestHandler<GetThemesQuery, ThemeListResponse>
{
    private readonly IThemeRepository _themeRepository;

    public GetThemesQueryHandler(IThemeRepository themeRepository)
    {
        _themeRepository = themeRepository;
    }

    public async Task<ThemeListResponse> Handle(GetThemesQuery request, CancellationToken cancellationToken)
    {
        var themes = await _themeRepository.GetAllThemesAsync();
        var activeTheme = await _themeRepository.GetActiveThemeAsync();

        var themeDtos = themes.Select(t => new ThemeDto
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
        }).ToList();

        var activeThemeDto = activeTheme != null ? new ThemeDto
        {
            Id = activeTheme.Id,
            Name = activeTheme.Name,
            Description = activeTheme.Description,
            IsDefault = activeTheme.IsDefault,
            IsActive = activeTheme.IsActive,
            CreatedAt = activeTheme.CreatedAt,
            UpdatedAt = activeTheme.UpdatedAt,
            Colors = new ThemeColorsDto
            {
                Primary = activeTheme.Colors.Primary,
                PrimaryLight = activeTheme.Colors.PrimaryLight,
                PrimaryDark = activeTheme.Colors.PrimaryDark,
                Secondary = activeTheme.Colors.Secondary,
                SecondaryLight = activeTheme.Colors.SecondaryLight,
                SecondaryDark = activeTheme.Colors.SecondaryDark,
                Accent = activeTheme.Colors.Accent,
                AccentLight = activeTheme.Colors.AccentLight,
                AccentDark = activeTheme.Colors.AccentDark,
                BackgroundPrimary = activeTheme.Colors.BackgroundPrimary,
                BackgroundSecondary = activeTheme.Colors.BackgroundSecondary,
                BackgroundTertiary = activeTheme.Colors.BackgroundTertiary
            }
        } : null;

        return new ThemeListResponse
        {
            Themes = themeDtos,
            ActiveTheme = activeThemeDto
        };
    }
}
