using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.Commands
{
    public class UpdateUserSettingsRequest
    {
        public Guid UserId { get; set; }
        public string? Language { get; set; }
        public string? Theme { get; set; }
        public bool? EmailNotifications { get; set; }
        public Guid? DefaultPrinterId { get; set; }
        public string? MeasurementSystem { get; set; }
        public string? TimeZone { get; set; }
        public Dictionary<string, string>? CustomSettings { get; set; }
    }

    public class UpdateUserSettingsCommandHandler
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<UpdateUserSettingsCommandHandler> _logger;

        public UpdateUserSettingsCommandHandler(PolyBucketDbContext context, ILogger<UpdateUserSettingsCommandHandler> logger)
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
            if (request.CustomSettings != null) settings.CustomSettings = request.CustomSettings;

            await _context.SaveChangesAsync();
        }
    }
} 