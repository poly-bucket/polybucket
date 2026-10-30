using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Http;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Repository;

namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Domain;

public interface ISetActiveThemeService
{
    Task<SetActiveThemeResponse> SetActiveThemeAsync(int themeId, CancellationToken cancellationToken = default);
}

public class SetActiveThemeService(ISetActiveThemeRepository repository) : ISetActiveThemeService
{
    public async Task<SetActiveThemeResponse> SetActiveThemeAsync(int themeId, CancellationToken cancellationToken = default)
    {
        if (!await repository.ThemeExistsAsync(themeId, cancellationToken))
            return new SetActiveThemeResponse { Success = false, Message = "Theme not found." };
        var ok = await repository.SetThemeAsActiveAsync(themeId, cancellationToken);
        return ok
            ? new SetActiveThemeResponse { Success = true, Message = "Theme activated successfully." }
            : new SetActiveThemeResponse { Success = false, Message = "Failed to activate theme." };
    }
}
