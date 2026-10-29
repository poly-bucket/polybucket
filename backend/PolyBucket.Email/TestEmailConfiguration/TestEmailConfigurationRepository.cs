using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using PolyBucket.Database.Entities.SystemSettings;

namespace PolyBucket.Email.TestEmailConfiguration;

public class TestEmailConfigurationRepository(PolyBucketDbContext context) : ITestEmailConfigurationRepository
{
    public async Task RecordSuccessfulTestAsync(DateTime testedAt, CancellationToken cancellationToken = default)
    {
        var value = testedAt.ToString("O", CultureInfo.InvariantCulture);
        var setting = await context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == EmailSystemSettingKeys.EmailLastSuccessfulTestAt, cancellationToken);

        if (setting == null)
        {
            context.SystemSettings.Add(new SystemSetting { Key = EmailSystemSettingKeys.EmailLastSuccessfulTestAt, Value = value });
        }
        else
        {
            setting.Value = value;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
