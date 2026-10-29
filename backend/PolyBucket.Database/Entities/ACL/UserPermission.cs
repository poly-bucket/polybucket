using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.ACL
{
    public class UserPermission : Auditable
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
        
        public bool IsGranted { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
        
        public Guid? GrantedByUserId { get; set; }
        public User? GrantedByUser { get; set; }
        
        public bool IsActive => ExpiresAt == null || ExpiresAt > DateTime.UtcNow;
    }
} 