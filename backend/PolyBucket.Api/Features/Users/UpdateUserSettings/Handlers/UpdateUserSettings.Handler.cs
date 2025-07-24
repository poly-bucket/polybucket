using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Domain;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Handlers
{
    public class UpdateUserSettingsCommandHandler(PolyBucketDbContext context, ILogger<UpdateUserSettingsCommandHandler> logger)
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<UpdateUserSettingsCommandHandler> _logger = logger;

        public async Task ExecuteAsync(UpdateUserSettingsCommand request)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == request.UserId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = request.UserId
                };
                _context.UserSettings.Add(settings);
            }

            if (request.Language != null) settings.Language = request.Language;
            if (request.Theme != null) settings.Theme = request.Theme;
            if (request.EmailNotifications.HasValue) settings.EmailNotifications = request.EmailNotifications.Value;
            if (request.DefaultPrinterId.HasValue) settings.DefaultPrinterId = request.DefaultPrinterId;
            if (request.MeasurementSystem != null) settings.MeasurementSystem = request.MeasurementSystem;
            if (request.TimeZone != null) settings.TimeZone = request.TimeZone;
            if (request.AutoRotateModels.HasValue) settings.AutoRotateModels = request.AutoRotateModels.Value;
            if (request.CustomSettings != null) settings.CustomSettings = request.CustomSettings;

            await _context.SaveChangesAsync();
        }
    }
} 