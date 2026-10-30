using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Repository;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Domain;

public class GetExtensibleThemesService(IGetExtensibleThemesRepository repository) : IGetExtensibleThemesService
{
    public async Task<IReadOnlyList<ThemeDefinition>> GetExtensibleThemesAsync(CancellationToken cancellationToken = default)
    {
        var themes = await repository.ThemeManager.GetAllThemesAsync();
        return themes.ToList();
    }
}
