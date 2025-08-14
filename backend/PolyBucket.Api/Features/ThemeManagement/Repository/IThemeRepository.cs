using PolyBucket.Api.Features.ThemeManagement.Domain;

namespace PolyBucket.Api.Features.ThemeManagement.Repository;

public interface IThemeRepository
{
    Task<List<Theme>> GetAllThemesAsync();
    Task<Theme?> GetThemeByIdAsync(int id);
    Task<Theme?> GetActiveThemeAsync();
    Task<Theme?> GetDefaultThemeAsync();
    Task<Theme> CreateThemeAsync(Theme theme);
    Task<Theme> UpdateThemeAsync(Theme theme);
    Task<bool> DeleteThemeAsync(int id);
    Task<bool> SetThemeAsActiveAsync(int id);
    Task<bool> SetThemeAsDefaultAsync(int id);
    Task<bool> ThemeExistsAsync(int id);
    Task<bool> ThemeNameExistsAsync(string name, int? excludeId = null);
}
