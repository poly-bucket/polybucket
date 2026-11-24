using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.Models.Common;

public static class ModelPrivacyResolver
{
    public static PrivacySettings Resolve(string? requestPrivacy, PrivacySettings serverDefault)
    {
        if (string.IsNullOrWhiteSpace(requestPrivacy))
        {
            return serverDefault;
        }

        return requestPrivacy.Trim().ToLowerInvariant() switch
        {
            "private" => PrivacySettings.Private,
            "unlisted" => PrivacySettings.Unlisted,
            "public" => PrivacySettings.Public,
            _ => PrivacySettings.Public
        };
    }

    public static bool ComputeIsPublic(PrivacySettings privacy) => privacy != PrivacySettings.Private;
}