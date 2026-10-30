using PolyBucket.Database.Entities.Themes;

namespace PolyBucket.Api.Features.ThemeManagement.GetThemes.Repository;

public interface IGetThemesRepository
{
    Task<List<Theme>> GetAllThemesAsync(CancellationToken cancellationToken = default);
    Task<Theme?> GetActiveThemeAsync(CancellationToken cancellationToken = default);
}
