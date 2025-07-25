using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain
{
    public class EnableTwoFactorAuthCommand
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
    }

    public class EnableTwoFactorAuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public IEnumerable<string>? BackupCodes { get; set; }
    }
} 