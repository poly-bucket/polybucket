using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Themes;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme.Repository;

public class CreateThemeRepository(PolyBucketDbContext context) : ICreateThemeRepository
{
    public Task<bool> ThemeNameExistsAsync(string name, CancellationToken cancellationToken = default) =>
        context.Themes.AnyAsync(t => t.Name == name, cancellationToken);

    public Task<List<Theme>> GetAllThemesAsync(CancellationToken cancellationToken = default) =>
        context.Themes.ToListAsync(cancellationToken);

    public async Task<Theme> CreateThemeAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        theme.CreatedAt = DateTime.UtcNow;
        theme.UpdatedAt = DateTime.UtcNow;
        context.Themes.Add(theme);
        await context.SaveChangesAsync(cancellationToken);
        return theme;
    }

    public async Task SetThemeAsDefaultAsync(int id, CancellationToken cancellationToken = default)
    {
        var all = await context.Themes.Where(t => t.IsDefault).ToListAsync(cancellationToken);
        foreach (var t in all) t.IsDefault = false;
        var theme = await context.Themes.FindAsync([id], cancellationToken);
        if (theme != null)
        {
            theme.IsDefault = true;
            theme.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
