using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetActiveThemes.Domain;

public interface IGetActiveThemesService { List<ActiveTheme> GetActiveThemes(); }
