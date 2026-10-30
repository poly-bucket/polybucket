using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Repository;

public interface IGetActiveExtensibleThemeComponentsRepository
{
    IThemeManager ThemeManager { get; }
}
