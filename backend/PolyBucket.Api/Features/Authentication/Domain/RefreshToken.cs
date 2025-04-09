using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Authentication.Domain
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;
        
        public Guid UserId { get; set; }
        public virtual PolyBucket.Api.Common.Models.User User { get; set; } = null!;
    }
} 