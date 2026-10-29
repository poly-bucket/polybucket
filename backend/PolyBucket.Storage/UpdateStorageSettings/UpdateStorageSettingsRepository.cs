using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Storage.UpdateStorageSettings;

public class UpdateStorageSettingsRepository(PolyBucketDbContext context) : IUpdateStorageSettingsRepository
{
    public async Task UpsertAsync(IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken = default)
    {
        var keys = values.Keys.ToList();
        var existing = await context.SystemSettings
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, cancellationToken);

        foreach (var (key, value) in values)
        {
            if (existing.TryGetValue(key, out var setting))
            {
                setting.Value = value;
            }
            else
            {
                context.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
            }
        }

        var systemSetup = await context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup != null && !systemSetup.IsStorageConfigured)
        {
            systemSetup.IsStorageConfigured = true;
            systemSetup.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
