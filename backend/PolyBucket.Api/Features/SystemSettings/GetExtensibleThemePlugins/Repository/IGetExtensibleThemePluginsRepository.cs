using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Repository;

public interface IGetExtensibleThemePluginsRepository
{
    IThemeManager ThemeManager { get; }
}
