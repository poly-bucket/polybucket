using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetAllRoles.Repository;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetAllRoles.Domain;

public interface IGetAllRolesService
{
    Task<PaginatedRolesResponse> GetAllRolesAsync(int page, int pageSize, string? searchTerm, string? sortBy, bool sortDescending);
}

public class GetAllRolesService(IRoleManagementService roleManagement, IGetAllRolesRepository repository) : IGetAllRolesService
{
    public async Task<PaginatedRolesResponse> GetAllRolesAsync(int page, int pageSize, string? searchTerm, string? sortBy, bool sortDescending)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        var roles = await roleManagement.GetAllRolesAsync();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            roles = roles.Where(r =>
                r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        roles = sortBy?.ToLower() switch
        {
            "name" => sortDescending ? roles.OrderByDescending(r => r.Name).ToList() : roles.OrderBy(r => r.Name).ToList(),
            "priority" => sortDescending ? roles.OrderByDescending(r => r.Priority).ToList() : roles.OrderBy(r => r.Priority).ToList(),
            "users" => sortDescending ? roles.OrderByDescending(r => r.Users.Count).ToList() : roles.OrderBy(r => r.Users.Count).ToList(),
            _ => sortDescending ? roles.OrderByDescending(r => r.Priority).ToList() : roles.OrderBy(r => r.Priority).ToList()
        };
        var totalCount = roles.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var pagedRoles = roles.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var roleDtos = new List<RoleDto>();
        foreach (var role in pagedRoles)
        {
            var permissions = await roleManagement.GetRolePermissionsAsync(role.Id);
            var userCount = await roleManagement.GetUserCountAsync(role.Id);
            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Priority = role.Priority,
                IsSystemRole = role.IsSystemRole,
                IsDefault = role.IsDefault,
                CanBeDeleted = role.CanBeDeleted,
                IsActive = role.IsActive,
                ParentRoleId = role.ParentRoleId,
                Permissions = permissions,
                UserCount = userCount,
                Color = role.Color
            });
        }
        return new PaginatedRolesResponse
        {
            Roles = roleDtos,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            }
        };
    }
}
