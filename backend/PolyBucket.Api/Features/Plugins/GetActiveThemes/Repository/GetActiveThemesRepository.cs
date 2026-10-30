using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetActiveThemes.Repository;

public class GetActiveThemesRepository(ThemePluginService themePluginService) : IGetActiveThemesRepository
{
    public List<ActiveTheme> GetActiveThemes() => themePluginService.GetActiveThemes();
}
