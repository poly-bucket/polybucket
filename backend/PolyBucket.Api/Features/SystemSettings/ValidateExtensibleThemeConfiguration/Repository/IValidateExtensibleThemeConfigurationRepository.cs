using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Repository;

public interface IValidateExtensibleThemeConfigurationRepository
{
    IThemeManager ThemeManager { get; }
}
