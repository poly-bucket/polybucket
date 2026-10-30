using PolyBucket.Database.Entities.Themes;

namespace PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Repository;

public interface IGetActiveThemeRepository
{
    Task<Theme?> GetActiveThemeAsync(CancellationToken cancellationToken = default);
}
