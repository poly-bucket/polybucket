namespace PolyBucket.Api.Features.Authentication.Domain;

public class TwoFactorAuthSettings
{
    public bool RequireTwoFactorForAdmins { get; set; } = false;
    public bool AllowBackupCodes { get; set; } = true;
    public int BackupCodeCount { get; set; } = 10;
    public int BackupCodeLength { get; set; } = 8;
    public int TokenExpirySeconds { get; set; } = 30;
    public string IssuerName { get; set; } = "PolyBucket";
    public bool RequireRecoveryEmail { get; set; } = false;
}
