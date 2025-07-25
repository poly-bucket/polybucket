using System;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain
{
    public class DisableTwoFactorAuthCommand
    {
        public Guid UserId { get; set; }
    }

    public class DisableTwoFactorAuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }
} 