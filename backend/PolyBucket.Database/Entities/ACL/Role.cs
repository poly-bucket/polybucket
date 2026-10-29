using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;
using System.Collections.Generic;

namespace PolyBucket.Database.Entities.ACL
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 0; // Higher number = higher priority
        public bool IsSystemRole { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public bool CanBeDeleted { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string Color { get; set; } = "#3B82F6"; // Default blue color
        
        // Hierarchy support
        public Guid? ParentRoleId { get; set; }
        public Role? ParentRole { get; set; }
        public ICollection<Role> ChildRoles { get; set; } = new List<Role>();
        
        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
} 