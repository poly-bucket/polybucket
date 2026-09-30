using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;

public class TestEmailConfigurationRepository(PolyBucketDbContext context) : ITestEmailConfigurationRepository
{
    public async Task RecordSuccessfulTestAsync(DateTime testedAt, CancellationToken cancellationToken = default)
    {
        var value = testedAt.ToString("O", CultureInfo.InvariantCulture);
        var setting = await context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.EmailLastSuccessfulTestAt, cancellationToken);

        if (setting == null)
        {
            context.SystemSettings.Add(new SystemSetting { Key = SystemSettingKeys.EmailLastSuccessfulTestAt, Value = value });
        }
        else
        {
            setting.Value = value;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
