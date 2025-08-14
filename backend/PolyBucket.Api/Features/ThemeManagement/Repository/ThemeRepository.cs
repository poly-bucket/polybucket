using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ThemeManagement.Domain;

namespace PolyBucket.Api.Features.ThemeManagement.Repository;

public class ThemeRepository : IThemeRepository
{
    private readonly PolyBucketDbContext _context;

    public ThemeRepository(PolyBucketDbContext context)
    {
        _context = context;
    }

    public async Task<List<Theme>> GetAllThemesAsync()
    {
        return await _context.Themes
            .Include(t => t.Colors)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Theme?> GetThemeByIdAsync(int id)
    {
        return await _context.Themes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Theme?> GetActiveThemeAsync()
    {
        return await _context.Themes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.IsActive);
    }

    public async Task<Theme?> GetDefaultThemeAsync()
    {
        return await _context.Themes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.IsDefault);
    }

    public async Task<Theme> CreateThemeAsync(Theme theme)
    {
        theme.CreatedAt = DateTime.UtcNow;
        theme.UpdatedAt = DateTime.UtcNow;
        
        _context.Themes.Add(theme);
        await _context.SaveChangesAsync();
        
        return theme;
    }

    public async Task<Theme> UpdateThemeAsync(Theme theme)
    {
        theme.UpdatedAt = DateTime.UtcNow;
        
        _context.Themes.Update(theme);
        await _context.SaveChangesAsync();
        
        return theme;
    }

    public async Task<bool> DeleteThemeAsync(int id)
    {
        var theme = await _context.Themes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (theme == null) return false;
        
        if (theme.IsDefault || theme.IsActive)
            return false; // Cannot delete default or active themes
        
        _context.Themes.Remove(theme);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> SetThemeAsActiveAsync(int id)
    {
        var theme = await _context.Themes.FindAsync(id);
        if (theme == null) return false;
        
        // Deactivate all other themes
        var allThemes = await _context.Themes.Where(t => t.IsActive).ToListAsync();
        foreach (var t in allThemes)
        {
            t.IsActive = false;
        }
        
        // Activate the selected theme
        theme.IsActive = true;
        theme.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetThemeAsDefaultAsync(int id)
    {
        var theme = await _context.Themes.FindAsync(id);
        if (theme == null) return false;
        
        // Remove default from all other themes
        var allThemes = await _context.Themes.Where(t => t.IsDefault).ToListAsync();
        foreach (var t in allThemes)
        {
            t.IsDefault = false;
        }
        
        // Set the selected theme as default
        theme.IsDefault = true;
        theme.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ThemeExistsAsync(int id)
    {
        return await _context.Themes.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> ThemeNameExistsAsync(string name, int? excludeId = null)
    {
        return await _context.Themes
            .Where(t => excludeId == null || t.Id != excludeId)
            .AnyAsync(t => t.Name == name);
    }
}
