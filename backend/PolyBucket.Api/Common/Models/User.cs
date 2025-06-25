using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Enums;
using PolyBucket.Api.Features.Users.Domain;
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
        public UserRole Role { get; set; }
        public string? Country { get; set; }
        public virtual ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
        public virtual UserSettings Settings { get; set; } = null!;
    }
} 