namespace PolyBucket.Api.Features.SystemSettings.Domain;

public static class SystemSettingKeys
{
    public const string AuthLoginMethod = "Auth:LoginMethod";
    public const string AuthAllowUsernameLogin = "Auth:AllowUsernameLogin";
    public const string AuthAllowEmailLogin = "Auth:AllowEmailLogin";
    public const string TokenAccessTokenExpiryHours = "Token:AccessTokenExpiryHours";
    public const string TokenRefreshTokenExpiryDays = "Token:RefreshTokenExpiryDays";
    public const string TokenEnableRefreshTokens = "Token:EnableRefreshTokens";
    public const string MarketplaceBaseUrl = "Marketplace:BaseUrl";
}
