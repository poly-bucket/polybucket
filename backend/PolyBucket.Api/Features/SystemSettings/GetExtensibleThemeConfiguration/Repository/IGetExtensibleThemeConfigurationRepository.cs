using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Repository;

public interface IGetExtensibleThemeConfigurationRepository
{
    IThemeManager ThemeManager { get; }
}
