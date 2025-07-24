using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain
{
    public class GetThemeSettingsQueryHandler(
        PolyBucketDbContext context,
        ILogger<GetThemeSettingsQueryHandler> logger) : IRequestHandler<GetThemeSettingsQuery, GetThemeSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<GetThemeSettingsQueryHandler> _logger = logger;

        public async Task<GetThemeSettingsResponse> Handle(GetThemeSettingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (systemSetup == null)
                {
                    return new GetThemeSettingsResponse
                    {
                        Success = false,
                        Message = "System setup not found. Please complete first-time setup first."
                    };
                }

                var themeSettings = new ThemeSettingsData
                {
                    PrimaryColor = systemSetup.PrimaryColor,
                    PrimaryLightColor = systemSetup.PrimaryLightColor,
                    PrimaryDarkColor = systemSetup.PrimaryDarkColor,
                    SecondaryColor = systemSetup.SecondaryColor,
                    SecondaryLightColor = systemSetup.SecondaryLightColor,
                    SecondaryDarkColor = systemSetup.SecondaryDarkColor,
                    AccentColor = systemSetup.AccentColor,
                    AccentLightColor = systemSetup.AccentLightColor,
                    AccentDarkColor = systemSetup.AccentDarkColor,
                    BackgroundPrimaryColor = systemSetup.BackgroundPrimaryColor,
                    BackgroundSecondaryColor = systemSetup.BackgroundSecondaryColor,
                    BackgroundTertiaryColor = systemSetup.BackgroundTertiaryColor,
                    IsThemeCustomized = systemSetup.IsThemeCustomized
                };

                return new GetThemeSettingsResponse
                {
                    Success = true,
                    Message = "Theme settings retrieved successfully",
                    ThemeSettings = themeSettings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve theme settings");
                return new GetThemeSettingsResponse
                {
                    Success = false,
                    Message = "Failed to retrieve theme settings. Please try again."
                };
            }
        }
    }
} 