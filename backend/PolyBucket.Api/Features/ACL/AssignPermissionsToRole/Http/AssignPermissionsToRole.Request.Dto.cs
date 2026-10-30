using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ACL.AssignPermissionsToRole.Http;

public class AssignPermissionsToRoleRequest
{
    [Required]
    public List<string> PermissionNames { get; set; } = new();
}
