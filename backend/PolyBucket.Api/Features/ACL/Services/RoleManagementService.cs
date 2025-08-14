using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Repository;
using PolyBucket.Api.Features.ACL.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Services
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;

        public RoleManagementService(IRoleRepository roleRepository, IPermissionService permissionService)
        {
            _roleRepository = roleRepository;
            _permissionService = permissionService;
        }

        public async Task<Role?> GetRoleByIdAsync(Guid id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<List<Role>> GetActiveRolesAsync()
        {
            return await _roleRepository.GetActiveRolesAsync();
        }

        public async Task<Role> CreateRoleAsync(Role role, List<string> initialPermissions)
        {
            if (await _roleRepository.ExistsByNameAsync(role.Name))
                throw new InvalidOperationException($"Role with name '{role.Name}' already exists");

            if (!await ValidateRoleHierarchyAsync(role.ParentRoleId, role.Priority))
                throw new InvalidOperationException("Invalid role hierarchy: child role must have lower priority than parent");

            var createdRole = await _roleRepository.CreateAsync(role);

            if (initialPermissions != null && initialPermissions.Any())
            {
                await _roleRepository.AssignPermissionsToRoleAsync(createdRole.Id, initialPermissions);
            }

            return createdRole;
        }

        public async Task<Role> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
        {
            var existingRole = await _roleRepository.GetByIdAsync(id);
            if (existingRole == null)
                throw new InvalidOperationException("Role not found");

            // if (existingRole.IsSystemRole && !existingRole.CanBeDeleted)
            //     throw new InvalidOperationException("System roles cannot be modified");

            if (!string.IsNullOrEmpty(request.Name) && request.Name != existingRole.Name)
            {
                if (await _roleRepository.ExistsByNameAsync(request.Name, id))
                    throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            if (request.ParentRoleId.HasValue && request.Priority.HasValue && !await ValidateRoleHierarchyAsync(request.ParentRoleId.Value, request.Priority.Value))
                throw new InvalidOperationException("Invalid role hierarchy: child role must have lower priority than parent");

            existingRole.Name = request.Name ?? existingRole.Name;
            existingRole.Description = request.Description ?? existingRole.Description;
            if (request.Priority.HasValue)
                existingRole.Priority = request.Priority.Value;
            existingRole.ParentRoleId = request.ParentRoleId ?? existingRole.ParentRoleId;
            if (request.IsDefault.HasValue)
                existingRole.IsDefault = request.IsDefault.Value;
            if (request.IsActive.HasValue)
                existingRole.IsActive = request.IsActive.Value;
            if (!string.IsNullOrEmpty(request.Color))
                existingRole.Color = request.Color;

            return await _roleRepository.UpdateAsync(existingRole);
        }

        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return false;

            if (role.IsSystemRole && !role.CanBeDeleted)
                throw new InvalidOperationException("System roles cannot be deleted");

            if (await _roleRepository.HasUsersAsync(id))
                throw new InvalidOperationException("Cannot delete role that has assigned users");

            return await _roleRepository.DeleteAsync(id);
        }

        public async Task<bool> CanManageRoleAsync(Guid userId, Guid roleId)
        {
            return await _permissionService.CanManageRoleAsync(userId, roleId);
        }

        public async Task<bool> CanDeleteRoleAsync(Guid userId, Guid roleId)
        {
            return await _permissionService.CanDeleteRoleAsync(userId, roleId);
        }

        public async Task<List<string>> GetRolePermissionsAsync(Guid roleId)
        {
            return await _roleRepository.GetRolePermissionsAsync(roleId);
        }

        public async Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissionNames)
        {
            if (permissionNames == null || !permissionNames.Any())
                return;

            await _roleRepository.AssignPermissionsToRoleAsync(roleId, permissionNames);
        }

        public async Task RemovePermissionsFromRoleAsync(Guid roleId, List<string> permissionNames)
        {
            if (permissionNames == null || !permissionNames.Any())
                return;

            await _roleRepository.RemovePermissionsFromRoleAsync(roleId, permissionNames);
        }

        public async Task<int> GetUserCountAsync(Guid roleId)
        {
            return await _roleRepository.GetUserCountAsync(roleId);
        }

        public async Task<bool> ValidateRoleHierarchyAsync(Guid? parentRoleId, int priority)
        {
            if (!parentRoleId.HasValue)
                return true;

            var parentRole = await _roleRepository.GetByIdAsync(parentRoleId.Value);
            if (parentRole == null)
                return false;

            return priority < parentRole.Priority;
        }

        public async Task<bool> IsRoleNameUniqueAsync(string name, Guid? excludeId = null)
        {
            return !await _roleRepository.ExistsByNameAsync(name, excludeId);
        }
    }
}
