using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetActiveThemes.Repository;

public interface IGetActiveThemesRepository { List<ActiveTheme> GetActiveThemes(); }
