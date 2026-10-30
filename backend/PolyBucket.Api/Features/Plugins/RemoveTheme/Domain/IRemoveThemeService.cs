using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.RemoveTheme.Domain;

public interface IRemoveThemeService
{
    Task<ThemeSimpleOutcome<ThemeApplicationResult>> RemoveAsync(string pluginId);
}
