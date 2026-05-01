namespace PolyBucket.Api.Features.Authentication.Account.Domain;

public class DeleteAccountRequest
{
    public string Password { get; set; } = string.Empty;
    public string? TwoFactorToken { get; set; }
    public string? BackupCode { get; set; }
}
