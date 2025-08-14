using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly PolyBucketDbContext _context;

        public RoleRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(Guid id)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(r => r.Users)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        public async Task<List<Role>> GetActiveRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        public async Task<Role> CreateAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return false;

            if (role.IsSystemRole && !role.CanBeDeleted)
                return false;

            if (await HasUsersAsync(id))
                return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Roles.AnyAsync(r => r.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _context.Roles.AnyAsync(r => r.Name == name && r.Id != excludeId.Value);
            
            return await _context.Roles.AnyAsync(r => r.Name == name);
        }

        public async Task<int> GetUserCountAsync(Guid roleId)
        {
            return await _context.Users.CountAsync(u => u.RoleId == roleId);
        }

        public async Task<List<string>> GetRolePermissionsAsync(Guid roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsGranted)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();
        }

        public async Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissionNames)
        {
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            var permissionsToAdd = new List<RolePermission>();

            foreach (var permissionName in permissionNames)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission == null) continue;

                var existingRolePermission = existingPermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
                if (existingRolePermission != null)
                {
                    existingRolePermission.IsGranted = true;
                }
                else
                {
                    permissionsToAdd.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permission.Id,
                        IsGranted = true
                    });
                }
            }

            if (permissionsToAdd.Any())
            {
                await _context.RolePermissions.AddRangeAsync(permissionsToAdd);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemovePermissionsFromRoleAsync(Guid roleId, List<string> permissionNames)
        {
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .ToListAsync();

            foreach (var permissionName in permissionNames)
            {
                var rolePermission = rolePermissions.FirstOrDefault(rp => rp.Permission.Name == permissionName);
                if (rolePermission != null)
                {
                    rolePermission.IsGranted = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Role>> GetRolesByPriorityRangeAsync(int minPriority, int maxPriority)
        {
            return await _context.Roles
                .Where(r => r.Priority >= minPriority && r.Priority <= maxPriority)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        public async Task<bool> HasUsersAsync(Guid roleId)
        {
            return await _context.Users.AnyAsync(u => u.RoleId == roleId);
        }
    }
}
