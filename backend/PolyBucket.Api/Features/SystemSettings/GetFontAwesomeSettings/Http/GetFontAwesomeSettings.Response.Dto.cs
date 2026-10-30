using PolyBucket.Api.Features.SystemSettings.Domain;
using System;
using System.ComponentModel;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http;

[DisplayName("GetFontAwesomeSettingsResponseDto")]
public class GetFontAwesomeSettingsResponseDto
{
    public Guid Id { get; set; }
    public bool IsProEnabled { get; set; }
    public bool HasProLicenseKey { get; set; }
    public string? ProKitUrl { get; set; }
    public bool UseProIcons { get; set; }
    public bool FallbackToFree { get; set; }
    public DateTime? LastLicenseCheck { get; set; }
    public bool IsLicenseValid { get; set; }
    public string? LicenseErrorMessage { get; set; }

    public static GetFontAwesomeSettingsResponseDto FromEntity(FontAwesomeSettings settings) => new()
    {
        Id = settings.Id,
        IsProEnabled = settings.IsProEnabled,
        HasProLicenseKey = !string.IsNullOrEmpty(settings.ProLicenseKey),
        ProKitUrl = settings.ProKitUrl,
        UseProIcons = settings.UseProIcons,
        FallbackToFree = settings.FallbackToFree,
        LastLicenseCheck = settings.LastLicenseCheck,
        IsLicenseValid = settings.IsLicenseValid,
        LicenseErrorMessage = settings.LicenseErrorMessage
    };
}

public class UpdateFontAwesomeSettingsRequestDto
{
    public bool IsProEnabled { get; set; }
    public string? ProLicenseKey { get; set; }
    public string? ProKitUrl { get; set; }
    public bool UseProIcons { get; set; } = true;
    public bool FallbackToFree { get; set; } = true;

    public FontAwesomeSettings ToEntity(Guid? existingId = null)
    {
        var entity = new FontAwesomeSettings
        {
            IsProEnabled = IsProEnabled,
            ProLicenseKey = ProLicenseKey,
            ProKitUrl = ProKitUrl,
            UseProIcons = UseProIcons,
            FallbackToFree = FallbackToFree
        };
        if (existingId.HasValue)
        {
            entity.Id = existingId.Value;
        }
        return entity;
    }
}
