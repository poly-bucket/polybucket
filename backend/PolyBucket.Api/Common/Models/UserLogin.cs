using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Common.Models
{
    public class UserLogin : BaseEntity
    {
        public string Email { get; set; } = null!;
        public bool? Successful { get; set; }
        public string? IpAddress { get; set; }
        public string UserAgent { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
} 