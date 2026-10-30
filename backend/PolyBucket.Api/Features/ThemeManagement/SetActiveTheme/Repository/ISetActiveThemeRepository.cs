namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Repository;

public interface ISetActiveThemeRepository
{
    Task<bool> ThemeExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SetThemeAsActiveAsync(int id, CancellationToken cancellationToken = default);
}
