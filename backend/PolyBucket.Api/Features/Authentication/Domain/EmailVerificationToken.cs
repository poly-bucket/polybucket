using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Authentication.Domain
{
    public class EmailVerificationToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public EmailVerificationPurpose Purpose { get; set; } = EmailVerificationPurpose.VerifyAddress;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
    }

    public enum EmailVerificationPurpose
    {
        VerifyAddress,
        ChangeAddress
    }
}
