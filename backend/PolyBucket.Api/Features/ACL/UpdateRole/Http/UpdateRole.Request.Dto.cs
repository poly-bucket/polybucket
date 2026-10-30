using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ACL.UpdateRole.Http;

public class UpdateRoleRequest
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 999)]
    public int? Priority { get; set; }

    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentRoleId { get; set; }

    [StringLength(7, MinimumLength = 7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #3B82F6)")]
    public string? Color { get; set; }
}
