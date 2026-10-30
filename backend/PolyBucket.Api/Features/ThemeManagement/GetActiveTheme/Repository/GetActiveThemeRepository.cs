using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Themes;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Repository;

public class GetActiveThemeRepository(PolyBucketDbContext context) : IGetActiveThemeRepository
{
    public Task<Theme?> GetActiveThemeAsync(CancellationToken cancellationToken = default) =>
        context.Themes.Include(t => t.Colors).FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
}
