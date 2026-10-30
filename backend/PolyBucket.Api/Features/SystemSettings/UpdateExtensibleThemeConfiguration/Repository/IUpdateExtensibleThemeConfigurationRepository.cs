using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Repository;

public interface IUpdateExtensibleThemeConfigurationRepository
{
    IThemeManager ThemeManager { get; }
}
