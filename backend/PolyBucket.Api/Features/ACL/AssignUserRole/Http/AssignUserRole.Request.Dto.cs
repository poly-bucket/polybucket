using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ACL.AssignUserRole.Http;

public class AssignUserRoleRequest
{
    [Required]
    public Guid RoleId { get; set; }
}
