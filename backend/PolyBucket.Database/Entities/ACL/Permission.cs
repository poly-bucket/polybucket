using PolyBucket.Database.Common;
using System.Collections.Generic;

namespace PolyBucket.Database.Entities.ACL
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsSystemPermission { get; set; } = false;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
} 