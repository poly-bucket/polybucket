using PolyBucket.Database.Common;
using System;

namespace PolyBucket.Database.Entities.Authentication
{
    public class PasswordResetToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
    }
} 