using PolyBucket.Api.Common.Entities;

namespace PolyBucket.Api.Features.SystemSettings.Domain
{
    public class FontAwesomeSettings : BaseEntity
    {
        public bool IsProEnabled { get; set; } = false;
        public string? ProLicenseKey { get; set; }
        public string? ProKitUrl { get; set; }
        public bool UseProIcons { get; set; } = true;
        public bool FallbackToFree { get; set; } = true;
        public DateTime? LastLicenseCheck { get; set; }
        public bool IsLicenseValid { get; set; } = false;
        public string? LicenseErrorMessage { get; set; }
    }
}
