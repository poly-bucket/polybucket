using PolyBucket.Database.Common;
using System;

namespace PolyBucket.Database.Entities.Authentication
{
    public class ExternalAuthProvider : BaseEntity
    {
        public string Provider { get; set; } = string.Empty; // "Google", "GitHub", "Discord", "Twitch"
        public string ExternalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        public Guid UserId { get; set; }
        public virtual PolyBucket.Database.Entities.Models.User User { get; set; } = null!;
    }
} 