using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ACL.RemovePermissionsFromRole.Http;

public class RemovePermissionsFromRoleRequest
{
    [Required]
    public List<string> PermissionNames { get; set; } = new();
}
