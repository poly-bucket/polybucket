using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using PolyBucket.Storage;

namespace PolyBucket.Api.Tests;

public static class TestStorageSettingsSeeder
{
    public static async Task SeedAsync(PolyBucketDbContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(TestEnvironment.StorageEndpoint)
            || !TestEnvironment.StoragePort.HasValue
            || string.IsNullOrWhiteSpace(TestEnvironment.StorageAccessKey)
            || string.IsNullOrWhiteSpace(TestEnvironment.StorageSecretKey)
            || string.IsNullOrWhiteSpace(TestEnvironment.StorageBucketName))
        {
            throw new InvalidOperationException("TestEnvironment storage is not initialized.");
        }

        var keys = new Dictionary<string, string>
        {
            [StorageSystemSettingKeys.Provider] = StorageProviderIds.Silo,
            [StorageSystemSettingKeys.Endpoint] = TestEnvironment.StorageEndpoint,
            [StorageSystemSettingKeys.Port] = TestEnvironment.StoragePort.Value.ToString(),
            [StorageSystemSettingKeys.UseSsl] = TestEnvironment.StorageUseSsl?.ToString() ?? "false",
            [StorageSystemSettingKeys.BucketName] = TestEnvironment.StorageBucketName,
            [StorageSystemSettingKeys.AccessKey] = TestEnvironment.StorageAccessKey,
            [StorageSystemSettingKeys.SecretKey] = TestEnvironment.StorageSecretKey,
            [StorageSystemSettingKeys.Region] = "us-east-1",
            [StorageSystemSettingKeys.ForcePathStyle] = "false"
        };

        foreach (var (key, value) in keys)
        {
            var existing = context.SystemSettings.FirstOrDefault(s => s.Key == key);
            if (existing == null)
            {
                context.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
            }
            else
            {
                existing.Value = value;
            }
        }

        var setup = await context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (setup != null)
        {
            setup.IsStorageConfigured = true;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
