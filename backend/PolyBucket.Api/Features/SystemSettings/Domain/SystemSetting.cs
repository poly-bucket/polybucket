using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class SystemSetting
{
    [Key]
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public static class SystemSettingKeys
{
    // Email Configuration Keys
    public const string EmailEnabled = "Email:Enabled";
    public const string EmailSmtpServer = "Email:SmtpServer";
    public const string EmailSmtpPort = "Email:SmtpPort";
    public const string EmailSmtpUsername = "Email:SmtpUsername";
    public const string EmailSmtpPassword = "Email:SmtpPassword";
    public const string EmailUseSsl = "Email:UseSsl";
    public const string EmailFromAddress = "Email:FromAddress";
    public const string EmailFromName = "Email:FromName";
    public const string EmailRequireVerification = "Email:RequireVerification";
    
    // Authentication Configuration Keys
    public const string AuthLoginMethod = "Auth:LoginMethod"; // "email", "username", "both"
    public const string AuthAllowUsernameLogin = "Auth:AllowUsernameLogin";
    public const string AuthAllowEmailLogin = "Auth:AllowEmailLogin";
    
    // Token Duration Configuration Keys
    public const string TokenAccessTokenExpiryHours = "Token:AccessTokenExpiryHours";
    public const string TokenRefreshTokenExpiryDays = "Token:RefreshTokenExpiryDays";
    public const string TokenEnableRefreshTokens = "Token:EnableRefreshTokens";
} 