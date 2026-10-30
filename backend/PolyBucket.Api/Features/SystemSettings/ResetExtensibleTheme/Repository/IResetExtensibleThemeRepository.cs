using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Repository;

public interface IResetExtensibleThemeRepository
{
    IThemeManager ThemeManager { get; }
}
