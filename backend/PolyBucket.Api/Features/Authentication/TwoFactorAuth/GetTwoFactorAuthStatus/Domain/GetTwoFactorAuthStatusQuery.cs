using System;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain
{
    public class GetTwoFactorAuthStatusQuery
    {
        public Guid UserId { get; set; }
    }

    public class GetTwoFactorAuthStatusResponse
    {
        public bool IsEnabled { get; set; }
        public bool IsInitialized { get; set; }
        public DateTime? EnabledAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public int RemainingBackupCodes { get; set; }
    }
} 