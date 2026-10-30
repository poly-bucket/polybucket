using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ACL.GrantUserPermission.Http;

public class GrantUserPermissionRequest
{
    [Required]
    public string Permission { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
