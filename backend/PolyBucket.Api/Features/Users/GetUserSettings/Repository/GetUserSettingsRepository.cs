using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Repository;

public class GetUserSettingsRepository(PolyBucketDbContext context) : IGetUserSettingsRepository
{
    public Task<UserSettings?> GetSettingsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<UserSettings> CreateDefaultSettingsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Language = "en",
            Theme = "dark",
            EmailNotifications = true,
            MeasurementSystem = "metric",
            TimeZone = "UTC",
        };

        context.UserSettings.Add(settings);
        await context.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
