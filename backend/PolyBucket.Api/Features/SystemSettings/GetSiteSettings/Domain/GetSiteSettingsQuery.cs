using MediatR;
using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Domain;

public class GetSiteSettingsQuery : IRequest<GetSiteSettingsResponse>
{
}

public class GetSiteSettingsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SiteSettingsData? Settings { get; set; }
}

public class SiteSettingsData
{
    public string SiteName { get; set; } = string.Empty;
    public string SiteDescription { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool AllowPublicBrowsing { get; set; } = true;
    public bool RequireLoginForUpload { get; set; } = true;
    public bool AllowUserRegistration { get; set; } = true;
    public bool RequireEmailVerification { get; set; }
    public bool DisableEmailSettings { get; set; }
    public long MaxFileSizeBytes { get; set; }
    public string AllowedFileTypes { get; set; } = string.Empty;
    public int MaxFilesPerUpload { get; set; }
    public bool EnableFileCompression { get; set; }
    public bool AutoGeneratePreviews { get; set; }
    public PrivacySettings DefaultModelPrivacy { get; set; } = PrivacySettings.Public;
    public bool AutoApproveModels { get; set; }
    public bool RequireModeration { get; set; } = true;
    public string DefaultUserRole { get; set; } = "User";
    public bool AllowCustomRoles { get; set; }
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool RequireStrongPasswords { get; set; } = true;
    public int PasswordMinLength { get; set; } = 8;
    public string DefaultTheme { get; set; } = "dark";
    public string DefaultLanguage { get; set; } = "en";
    public bool ShowAdvancedOptions { get; set; }
    public bool EnableFederation { get; set; }
    public string InstanceName { get; set; } = string.Empty;
    public string InstanceDescription { get; set; } = string.Empty;
    public string? AdminContact { get; set; }
}
