namespace PolyBucket.Api.Features.ACL.GetUserPermissions.Http;

public class UserPermissionsResponseDto
{
    public Guid UserId { get; set; }
    public UserRoleSummaryDto Role { get; set; } = null!;
    public List<string> EffectivePermissions { get; set; } = new();
    public List<PermissionOverrideResponseDto> PermissionOverrides { get; set; } = new();
}

public class UserRoleSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class PermissionOverrideResponseDto
{
    public Guid Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? GrantedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
}
