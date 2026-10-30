using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Domain;

public interface IGetExtensibleThemesService
{
    Task<IReadOnlyList<ThemeDefinition>> GetExtensibleThemesAsync(CancellationToken cancellationToken = default);
}
