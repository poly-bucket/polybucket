using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.Models.Common;

public static class ModelLicenseParser
{
    public static LicenseTypes? Parse(string? license)
    {
        return license?.Trim().ToLowerInvariant() switch
        {
            "mit" => LicenseTypes.MIT,
            "gpl" => LicenseTypes.GPLv3,
            "creative commons" or "ccby4" or "cc-by-4.0" => LicenseTypes.CCBy4,
            "commercial" => LicenseTypes.MIT,
            "custom" => LicenseTypes.MIT,
            _ => null
        };
    }

    public static bool IsCustomLicenseRequest(string? license)
    {
        return string.Equals(license?.Trim(), "custom", StringComparison.OrdinalIgnoreCase);
    }
}