using System;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Models.Auth
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime Expires { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public string ReasonRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => RevokedByIp != null;
        public bool IsActive => !IsRevoked && !IsExpired;
        
        // Navigation properties
        public User User { get; set; }
    }
} 