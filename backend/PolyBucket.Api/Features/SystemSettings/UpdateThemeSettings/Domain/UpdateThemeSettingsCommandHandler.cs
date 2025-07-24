using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain
{
    public class UpdateThemeSettingsCommandHandler(
        PolyBucketDbContext context,
        ILogger<UpdateThemeSettingsCommandHandler> logger) : IRequestHandler<UpdateThemeSettingsCommand, UpdateThemeSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<UpdateThemeSettingsCommandHandler> _logger = logger;

        public async Task<UpdateThemeSettingsResponse> Handle(UpdateThemeSettingsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (systemSetup == null)
                {
                    return new UpdateThemeSettingsResponse
                    {
                        Success = false,
                        Message = "System setup not found. Please complete first-time setup first."
                    };
                }

                // Update theme colors
                systemSetup.PrimaryColor = request.PrimaryColor;
                systemSetup.PrimaryLightColor = request.PrimaryLightColor;
                systemSetup.PrimaryDarkColor = request.PrimaryDarkColor;
                systemSetup.SecondaryColor = request.SecondaryColor;
                systemSetup.SecondaryLightColor = request.SecondaryLightColor;
                systemSetup.SecondaryDarkColor = request.SecondaryDarkColor;
                systemSetup.AccentColor = request.AccentColor;
                systemSetup.AccentLightColor = request.AccentLightColor;
                systemSetup.AccentDarkColor = request.AccentDarkColor;
                systemSetup.BackgroundPrimaryColor = request.BackgroundPrimaryColor;
                systemSetup.BackgroundSecondaryColor = request.BackgroundSecondaryColor;
                systemSetup.BackgroundTertiaryColor = request.BackgroundTertiaryColor;
                systemSetup.IsThemeCustomized = true;
                
                // Update timestamps
                systemSetup.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Theme settings updated successfully");

                return new UpdateThemeSettingsResponse
                {
                    Success = true,
                    Message = "Theme settings updated successfully",
                    IsThemeCustomized = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update theme settings");
                return new UpdateThemeSettingsResponse
                {
                    Success = false,
                    Message = "Failed to update theme settings. Please try again."
                };
            }
        }
    }
} 