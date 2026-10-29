using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Domain;

public class GetSiteSettingsQueryHandler(
    PolyBucketDbContext context,
    ILogger<GetSiteSettingsQueryHandler> logger) : IRequestHandler<GetSiteSettingsQuery, GetSiteSettingsResponse>
{
    private readonly PolyBucketDbContext _context = context;
    private readonly ILogger<GetSiteSettingsQueryHandler> _logger = logger;

    public async Task<GetSiteSettingsResponse> Handle(GetSiteSettingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var systemSetup = await _context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            if (systemSetup == null)
            {
                systemSetup = new SystemSetup();
            }

            return new GetSiteSettingsResponse
            {
                Success = true,
                Message = "Site settings retrieved successfully",
                Settings = MapFromEntity(systemSetup)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve site settings");
            return new GetSiteSettingsResponse
            {
                Success = false,
                Message = "Failed to retrieve site settings"
            };
        }
    }

    private static SiteSettingsData MapFromEntity(SystemSetup entity) => new()
    {
        SiteName = entity.SiteName,
        SiteDescription = entity.SiteDescription,
        ContactEmail = entity.ContactEmail,
        AllowPublicBrowsing = entity.AllowPublicBrowsing,
        RequireLoginForUpload = entity.RequireLoginForUpload,
        AllowUserRegistration = entity.AllowUserRegistration,
        RequireEmailVerification = entity.RequireEmailVerification,
        DisableEmailSettings = entity.DisableEmailSettings,
        MaxFileSizeBytes = entity.MaxFileSizeBytes,
        AllowedFileTypes = entity.AllowedFileTypes,
        MaxFilesPerUpload = entity.MaxFilesPerUpload,
        EnableFileCompression = entity.EnableFileCompression,
        AutoGeneratePreviews = entity.AutoGeneratePreviews,
        DefaultModelPrivacy = entity.DefaultModelPrivacy,
        AutoApproveModels = entity.AutoApproveModels,
        RequireModeration = entity.RequireModeration,
        DefaultUserRole = entity.DefaultUserRole,
        AllowCustomRoles = entity.AllowCustomRoles,
        MaxFailedLoginAttempts = entity.MaxFailedLoginAttempts,
        LockoutDurationMinutes = entity.LockoutDurationMinutes,
        RequireStrongPasswords = entity.RequireStrongPasswords,
        PasswordMinLength = entity.PasswordMinLength,
        DefaultTheme = entity.DefaultTheme,
        DefaultLanguage = entity.DefaultLanguage,
        ShowAdvancedOptions = entity.ShowAdvancedOptions,
        EnableFederation = entity.EnableFederation,
        InstanceName = entity.InstanceName,
        InstanceDescription = entity.InstanceDescription,
        AdminContact = string.IsNullOrEmpty(entity.AdminContact) ? null : entity.AdminContact
    };
}
