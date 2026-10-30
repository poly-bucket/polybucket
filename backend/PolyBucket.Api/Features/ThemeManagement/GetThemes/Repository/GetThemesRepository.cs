using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Themes;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.ThemeManagement.GetThemes.Repository;

public class GetThemesRepository(PolyBucketDbContext context) : IGetThemesRepository
{
    public Task<List<Theme>> GetAllThemesAsync(CancellationToken cancellationToken = default) =>
        context.Themes.Include(t => t.Colors).OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public Task<Theme?> GetActiveThemeAsync(CancellationToken cancellationToken = default) =>
        context.Themes.Include(t => t.Colors).FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
}
