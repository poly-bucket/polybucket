using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Repository;

public interface IGetActiveExtensibleThemeRepository
{
    IThemeManager ThemeManager { get; }
}
