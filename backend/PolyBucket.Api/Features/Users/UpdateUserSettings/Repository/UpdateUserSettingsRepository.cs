using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Repository;

public class UpdateUserSettingsRepository(PolyBucketDbContext context) : IUpdateUserSettingsRepository
{
    public async Task ApplyUpdateAsync(UpdateUserSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var settings = await context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = request.UserId
            };
            context.UserSettings.Add(settings);
        }

        if (request.Language != null) settings.Language = request.Language;
        if (request.Theme != null) settings.Theme = request.Theme;
        if (request.EmailNotifications.HasValue) settings.EmailNotifications = request.EmailNotifications.Value;
        if (request.DefaultPrinterId.HasValue) settings.DefaultPrinterId = request.DefaultPrinterId;
        if (request.MeasurementSystem != null) settings.MeasurementSystem = request.MeasurementSystem;
        if (request.TimeZone != null) settings.TimeZone = request.TimeZone;
        if (request.AutoRotateModels.HasValue) settings.AutoRotateModels = request.AutoRotateModels.Value;
        if (request.DashboardViewType != null) settings.DashboardViewType = request.DashboardViewType;
        if (request.CardSize != null) settings.CardSize = request.CardSize;
        if (request.CardSpacing != null) settings.CardSpacing = request.CardSpacing;
        if (request.GridColumns.HasValue) settings.GridColumns = request.GridColumns.Value;
        if (request.CustomSettings != null) settings.CustomSettings = request.CustomSettings;

        await context.SaveChangesAsync(cancellationToken);
    }
}
