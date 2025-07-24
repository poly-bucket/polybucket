using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Common.Models
{
    public class User : Auditable
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Salt { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public Guid? RoleId { get; set; }
        public virtual Role? Role { get; set; }
        public string? Country { get; set; }
        public bool IsBanned { get; set; } = false;
        public DateTime? BannedAt { get; set; }
        public Guid? BannedById { get; set; }
        public virtual User? BannedByUser { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiresAt { get; set; }
        public bool HasCompletedFirstTimeSetup { get; set; } = false;
        public bool RequiresPasswordChange { get; set; } = false;
        public string? Avatar { get; set; }
        public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
        public virtual UserSettings Settings { get; set; } = null!;
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
} 