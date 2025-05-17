using Database;
using Microsoft.EntityFrameworkCore;
using Core.Models.Users.Settings;

namespace Api.Controllers.UserSettings.Domain;

public interface IUpdateUserSettingsService
{
    Task ExecuteAsync(UpdateUserSettingsRequest request);
}

public class UpdateUserSettingsService : IUpdateUserSettingsService
{
    private readonly Context _context;
    private readonly ILogger<UpdateUserSettingsService> _logger;

    public UpdateUserSettingsService(Context context, ILogger<UpdateUserSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync(UpdateUserSettingsRequest request)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        if (settings == null)
        {
            settings = new Core.Models.Users.Settings.UserSettings
            {
                UserId = request.UserId
            };
            _context.UserSettings.Add(settings);
        }

        // Update only the properties that are provided
        if (request.Language != null) settings.Language = request.Language;
        if (request.Theme != null) settings.Theme = request.Theme;
        if (request.EmailNotifications.HasValue) settings.EmailNotifications = request.EmailNotifications.Value;
        if (request.DefaultPrinterId.HasValue) settings.DefaultPrinterId = request.DefaultPrinterId;
        if (request.MeasurementSystem != null) settings.MeasurementSystem = request.MeasurementSystem;
        if (request.TimeZone != null) settings.TimeZone = request.TimeZone;
        if (request.CustomSettings != null) settings.CustomSettings = request.CustomSettings;

        await _context.SaveChangesAsync();
    }
}