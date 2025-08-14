using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Repository
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id);
        Task<Role?> GetByNameAsync(string name);
        Task<List<Role>> GetAllAsync();
        Task<List<Role>> GetActiveRolesAsync();
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
        Task<int> GetUserCountAsync(Guid roleId);
        Task<List<string>> GetRolePermissionsAsync(Guid roleId);
        Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissionNames);
        Task RemovePermissionsFromRoleAsync(Guid roleId, List<string> permissionNames);
        Task<List<Role>> GetRolesByPriorityRangeAsync(int minPriority, int maxPriority);
        Task<bool> HasUsersAsync(Guid roleId);
    }
}
