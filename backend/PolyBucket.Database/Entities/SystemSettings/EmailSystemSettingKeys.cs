namespace PolyBucket.Database.Entities.SystemSettings;

public static class EmailSystemSettingKeys
{
    public const string EmailEnabled = "Email:Enabled";
    public const string EmailSmtpServer = "Email:SmtpServer";
    public const string EmailSmtpPort = "Email:SmtpPort";
    public const string EmailSmtpUsername = "Email:SmtpUsername";
    public const string EmailSmtpPassword = "Email:SmtpPassword";
    public const string EmailUseSsl = "Email:UseSsl";
    public const string EmailFromAddress = "Email:FromAddress";
    public const string EmailFromName = "Email:FromName";
    public const string EmailRequireVerification = "Email:RequireVerification";
    public const string EmailTransport = "Email:Transport";
    public const string EmailSecurity = "Email:Security";
    public const string EmailReplyTo = "Email:ReplyTo";
    public const string EmailPublicBaseUrl = "Email:PublicBaseUrl";
    public const string EmailAllowInvalidCertificates = "Email:AllowInvalidCertificates";
    public const string EmailLastSuccessfulTestAt = "Email:LastSuccessfulTestAt";
}
