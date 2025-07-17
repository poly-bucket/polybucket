using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.ACL.Domain
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