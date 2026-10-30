using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Common.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Repository;

public interface IGetExtensibleThemesRepository
{
    IThemeManager ThemeManager { get; }
}
