using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.ACL.GetAllRoles.Http;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsDefault { get; set; }
    public bool CanBeDeleted { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentRoleId { get; set; }
    public List<string> Permissions { get; set; } = new();
    [JsonPropertyName("userCount")]
    public int UserCount { get; set; }
    public string Color { get; set; } = "#3B82F6";
}

public class PaginatedRolesResponse
{
    public List<RoleDto> Roles { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
