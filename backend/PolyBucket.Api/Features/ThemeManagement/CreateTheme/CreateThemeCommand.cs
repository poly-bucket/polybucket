using MediatR;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Features.ThemeManagement.Repository;

namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme;

public record CreateThemeCommand : IRequest<ThemeResponse>
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsDefault { get; init; } = false;
    public ThemeColorsDto Colors { get; init; } = null!;
}

public class CreateThemeCommandHandler : IRequestHandler<CreateThemeCommand, ThemeResponse>
{
    private readonly IThemeRepository _themeRepository;

    public CreateThemeCommandHandler(IThemeRepository themeRepository)
    {
        _themeRepository = themeRepository;
    }

    public async Task<ThemeResponse> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
    {
        // Validate theme name uniqueness
        if (await _themeRepository.ThemeNameExistsAsync(request.Name))
        {
            return new ThemeResponse
            {
                Success = false,
                Message = "A theme with this name already exists."
            };
        }

        // If this is the first theme, make it default and active
        var existingThemes = await _themeRepository.GetAllThemesAsync();
        var isFirstTheme = !existingThemes.Any();

        var theme = new Theme
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = isFirstTheme || request.IsDefault,
            IsActive = isFirstTheme,
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

        // If setting as default, remove default from other themes
        if (request.IsDefault && !isFirstTheme)
        {
            await _themeRepository.SetThemeAsDefaultAsync(0); // This will be updated after creation
        }

        var createdTheme = await _themeRepository.CreateThemeAsync(theme);

        // If this was set as default, now set it properly
        if (request.IsDefault && !isFirstTheme)
        {
            await _themeRepository.SetThemeAsDefaultAsync(createdTheme.Id);
            createdTheme.IsDefault = true;
        }

        var themeDto = new ThemeDto
        {
            Id = createdTheme.Id,
            Name = createdTheme.Name,
            Description = createdTheme.Description,
            IsDefault = createdTheme.IsDefault,
            IsActive = createdTheme.IsActive,
            CreatedAt = createdTheme.CreatedAt,
            UpdatedAt = createdTheme.UpdatedAt,
            Colors = new ThemeColorsDto
            {
                Primary = createdTheme.Colors.Primary,
                PrimaryLight = createdTheme.Colors.PrimaryLight,
                PrimaryDark = createdTheme.Colors.PrimaryDark,
                Secondary = createdTheme.Colors.Secondary,
                SecondaryLight = createdTheme.Colors.SecondaryLight,
                SecondaryDark = createdTheme.Colors.SecondaryDark,
                Accent = createdTheme.Colors.Accent,
                AccentLight = createdTheme.Colors.AccentLight,
                AccentDark = createdTheme.Colors.AccentDark,
                BackgroundPrimary = createdTheme.Colors.BackgroundPrimary,
                BackgroundSecondary = createdTheme.Colors.BackgroundSecondary,
                BackgroundTertiary = createdTheme.Colors.BackgroundTertiary
            }
        };

        return new ThemeResponse
        {
            Success = true,
            Message = "Theme created successfully.",
            Theme = themeDto
        };
    }
}
