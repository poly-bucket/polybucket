using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Core.Models.Roles
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class CreateRoleRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
    }
    
    public class UpdateRoleRequest
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
    }
}