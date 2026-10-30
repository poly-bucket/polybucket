using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Repository;

public interface IGetExtensibleThemeComponentsRepository
{
    IThemeManager ThemeManager { get; }
}
