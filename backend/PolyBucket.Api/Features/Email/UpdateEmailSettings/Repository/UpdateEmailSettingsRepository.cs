using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;

public class UpdateEmailSettingsRepository(PolyBucketDbContext context) : IUpdateEmailSettingsRepository
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
        if (systemSetup != null && !systemSetup.IsEmailConfigured)
        {
            systemSetup.IsEmailConfigured = true;
            systemSetup.UpdatedAt = System.DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
