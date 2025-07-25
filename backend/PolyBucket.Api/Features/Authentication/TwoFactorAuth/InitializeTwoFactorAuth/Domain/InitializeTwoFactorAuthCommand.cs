using System;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain
{
    public class InitializeTwoFactorAuthCommand
    {
        public Guid UserId { get; set; }
    }

    public class InitializeTwoFactorAuthResponse
    {
        public string QrCodeUrl { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
    }
} 