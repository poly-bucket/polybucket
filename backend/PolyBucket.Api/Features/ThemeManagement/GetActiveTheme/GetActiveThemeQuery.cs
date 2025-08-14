using MediatR;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Features.ThemeManagement.Repository;

namespace PolyBucket.Api.Features.ThemeManagement.GetActiveTheme;

public record GetActiveThemeQuery : IRequest<ThemeDto?>;

public class GetActiveThemeQueryHandler : IRequestHandler<GetActiveThemeQuery, ThemeDto?>
{
    private readonly IThemeRepository _themeRepository;

    public GetActiveThemeQueryHandler(IThemeRepository themeRepository)
    {
        _themeRepository = themeRepository;
    }

    public async Task<ThemeDto?> Handle(GetActiveThemeQuery request, CancellationToken cancellationToken)
    {
        var activeTheme = await _themeRepository.GetActiveThemeAsync();
        
        if (activeTheme == null) return null;

        return new ThemeDto
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
        };
    }
}
