using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.GetActiveThemes.Repository;

namespace PolyBucket.Api.Features.Plugins.GetActiveThemes.Domain;

public class GetActiveThemesService(IGetActiveThemesRepository repository) : IGetActiveThemesService
{
    public List<ActiveTheme> GetActiveThemes() => repository.GetActiveThemes();
}
