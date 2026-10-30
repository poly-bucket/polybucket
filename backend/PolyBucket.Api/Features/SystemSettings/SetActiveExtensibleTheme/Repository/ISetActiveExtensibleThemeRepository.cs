using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Repository;

public interface ISetActiveExtensibleThemeRepository
{
    IThemeManager ThemeManager { get; }
}
