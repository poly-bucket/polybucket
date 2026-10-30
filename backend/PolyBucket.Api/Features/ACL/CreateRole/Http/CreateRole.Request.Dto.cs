using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.ACL.CreateRole.Http;

public class CreateRoleRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 999)]
    public int Priority { get; set; } = 100;

    public bool IsSystemRole { get; set; }
    public bool IsDefault { get; set; }
    public Guid? ParentRoleId { get; set; }
    public List<string> InitialPermissions { get; set; } = new();

    [StringLength(7, MinimumLength = 7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #3B82F6)")]
    public string Color { get; set; } = "#3B82F6";
}
