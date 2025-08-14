using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.SystemSettings.Repository
{
    public class FontAwesomeSettingsRepository : IFontAwesomeSettingsRepository
    {
        private readonly PolyBucketDbContext _context;

        public FontAwesomeSettingsRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<FontAwesomeSettings?> GetSettingsAsync()
        {
            return await _context.Set<FontAwesomeSettings>().FirstOrDefaultAsync();
        }

        public async Task<FontAwesomeSettings> CreateSettingsAsync(FontAwesomeSettings settings)
        {
            _context.Set<FontAwesomeSettings>().Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<FontAwesomeSettings> UpdateSettingsAsync(FontAwesomeSettings settings)
        {
            _context.Set<FontAwesomeSettings>().Update(settings);
            await _context.SaveChangesAsync();
            return settings;
        }
    }
}
