namespace PolyBucket.Api.Features.Authentication.Login.Domain
{
    public class LoginCommandResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public bool RequiresPasswordChange { get; set; } = false;
        public bool RequiresFirstTimeSetup { get; set; } = false;
        public string? SetupStep { get; set; } = null;
        public bool RequiresTwoFactor { get; set; } = false;
        public string? TwoFactorToken { get; set; } = null;
    }
} 