using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Services
{
    public interface IRoleManagementService
    {
        Task<Role?> GetRoleByIdAsync(Guid id);
        Task<List<Role>> GetAllRolesAsync();
        Task<List<Role>> GetActiveRolesAsync();
        Task<Role> CreateRoleAsync(Role role, List<string> initialPermissions);
        Task<Role> UpdateRoleAsync(Guid id, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(Guid id);
        Task<bool> CanManageRoleAsync(Guid userId, Guid roleId);
        Task<bool> CanDeleteRoleAsync(Guid userId, Guid roleId);
        Task<List<string>> GetRolePermissionsAsync(Guid roleId);
        Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissionNames);
        Task RemovePermissionsFromRoleAsync(Guid roleId, List<string> permissionNames);
        Task<int> GetUserCountAsync(Guid roleId);
        Task<bool> ValidateRoleHierarchyAsync(Guid? parentRoleId, int priority);
        Task<bool> IsRoleNameUniqueAsync(string name, Guid? excludeId = null);
    }
}
