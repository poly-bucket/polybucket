using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.ACL
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;
        
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
        
        public bool IsGranted { get; set; } = true;
        public Guid? GrantedByUserId { get; set; }
        public User? GrantedByUser { get; set; }
    }
} 