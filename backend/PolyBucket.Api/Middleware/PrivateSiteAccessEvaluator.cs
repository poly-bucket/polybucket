namespace PolyBucket.Api.Middleware;

public static class PrivateSiteAccessEvaluator
{
    public static readonly string[] AllowlistedPrefixes =
    {
        "/api/auth",
        "/api/systemsetup",
        "/api/system-settings/theme",
        "/api/system-settings/extensible-theme",
        "/health",
        "/swagger",
        "/openapi"
    };

    public static bool IsAllowlisted(PathString path)
    {
        foreach (var prefix in AllowlistedPrefixes)
        {
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static bool ShouldAllowRequest(PathString path, bool isAuthenticated, bool allowPublicBrowsing)
    {
        return allowPublicBrowsing
            || isAuthenticated
            || IsAllowlisted(path);
    }
}
