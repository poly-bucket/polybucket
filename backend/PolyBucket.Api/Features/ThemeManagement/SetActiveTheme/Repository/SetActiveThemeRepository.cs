using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Repository;

public class SetActiveThemeRepository(PolyBucketDbContext context) : ISetActiveThemeRepository
{
    public Task<bool> ThemeExistsAsync(int id, CancellationToken cancellationToken = default) =>
        context.Themes.AnyAsync(t => t.Id == id, cancellationToken);

    public async Task<bool> SetThemeAsActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var theme = await context.Themes.FindAsync([id], cancellationToken);
        if (theme == null) return false;
        var active = await context.Themes.Where(t => t.IsActive).ToListAsync(cancellationToken);
        foreach (var t in active) t.IsActive = false;
        theme.IsActive = true;
        theme.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
