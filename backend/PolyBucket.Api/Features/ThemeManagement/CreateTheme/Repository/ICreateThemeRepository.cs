using PolyBucket.Database.Entities.Themes;

namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme.Repository;

public interface ICreateThemeRepository
{
    Task<bool> ThemeNameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Theme>> GetAllThemesAsync(CancellationToken cancellationToken = default);
    Task<Theme> CreateThemeAsync(Theme theme, CancellationToken cancellationToken = default);
    Task SetThemeAsDefaultAsync(int id, CancellationToken cancellationToken = default);
}
