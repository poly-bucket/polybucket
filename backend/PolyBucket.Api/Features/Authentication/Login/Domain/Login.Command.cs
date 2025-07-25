namespace PolyBucket.Api.Features.Authentication.Login.Domain
{
    public class LoginCommand
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string? TwoFactorToken { get; set; }
        public string? BackupCode { get; set; }
        public string Email { get; set; } = string.Empty;
    }
} 