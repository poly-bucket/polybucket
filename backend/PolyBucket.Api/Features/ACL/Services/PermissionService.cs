using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Services
{
    public class PermissionService(PolyBucketDbContext context) : IPermissionService
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly Dictionary<Guid, List<string>> _userPermissionCache = new();
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);
        private readonly Dictionary<Guid, DateTime> _cacheTimestamps = new();

        #region Permission Checking

        public async Task<bool> HasPermissionAsync(Guid userId, string permission)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            // Admins have all permissions (supremacy rule)
            if (await IsAdminAsync(userId))
                return true;

            // Check user-specific permission overrides first (they take precedence)
            var userOverride = await _context.UserPermissions
                .Include(up => up.Permission)
                .FirstOrDefaultAsync(up => up.UserId == userId && 
                                          up.Permission.Name == permission && 
                                          up.IsActive);

            if (userOverride != null)
                return userOverride.IsGranted;

            // Check role permissions
            if (user.Role != null)
            {
                var rolePermission = await _context.RolePermissions
                    .Include(rp => rp.Permission)
                    .FirstOrDefaultAsync(rp => rp.RoleId == user.Role.Id && 
                                              rp.Permission.Name == permission &&
                                              rp.IsGranted);

                if (rolePermission != null)
                    return true;

                // Check inherited permissions from parent roles
                return await HasInheritedPermissionAsync(user.Role.Id, permission);
            }

            return false;
        }

        public async Task<bool> HasAnyPermissionAsync(Guid userId, params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (await HasPermissionAsync(userId, permission))
                    return true;
            }
            return false;
        }

        public async Task<bool> HasAllPermissionsAsync(Guid userId, params string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!await HasPermissionAsync(userId, permission))
                    return false;
            }
            return true;
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            // Check cache first
            if (_userPermissionCache.ContainsKey(userId) && 
                _cacheTimestamps.ContainsKey(userId) &&
                DateTime.UtcNow - _cacheTimestamps[userId] < _cacheExpiry)
            {
                return _userPermissionCache[userId];
            }

            var permissions = new HashSet<string>();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<string>();

            // Admins get all permissions
            if (await IsAdminAsync(userId))
            {
                var allPermissions = await _context.Permissions
                    .Where(p => p.IsActive)
                    .Select(p => p.Name)
                    .ToListAsync();
                
                _userPermissionCache[userId] = allPermissions;
                _cacheTimestamps[userId] = DateTime.UtcNow;
                return allPermissions;
            }

            // Get role permissions (including inherited)
            if (user.Role != null)
            {
                var rolePermissions = await GetRolePermissionsRecursiveAsync(user.Role.Id);
                foreach (var perm in rolePermissions)
                    permissions.Add(perm);
            }

            // Apply user-specific overrides
            var userOverrides = await _context.UserPermissions
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId && up.IsActive)
                .ToListAsync();

            foreach (var userOverride in userOverrides)
            {
                if (userOverride.IsGranted)
                    permissions.Add(userOverride.Permission.Name);
                else
                    permissions.Remove(userOverride.Permission.Name);
            }

            var result = permissions.ToList();
            _userPermissionCache[userId] = result;
            _cacheTimestamps[userId] = DateTime.UtcNow;
            
            return result;
        }

        public async Task<bool> CanUserModerateUserAsync(Guid moderatorId, Guid targetUserId)
        {
            var moderator = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == moderatorId);
            var target = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == targetUserId);

            if (moderator == null || target == null) return false;

            // Admins can moderate anyone except other admins (unless they are also admin)
            var isModeratorAdmin = await IsAdminAsync(moderatorId);
            var isTargetAdmin = await IsAdminAsync(targetUserId);

            if (isTargetAdmin && !isModeratorAdmin) return false; // Only admins can moderate admins
            if (isTargetAdmin && isModeratorAdmin) return true; // Admins can moderate other admins

            // Check if moderator has moderation permissions
            return await HasPermissionAsync(moderatorId, PermissionConstants.MODERATION_MODERATE_USERS);
        }

        #endregion

        #region Role Management

        public async Task<bool> IsAdminAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            return user?.Role?.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true ||
                   user?.Role?.Priority >= 1000; // Admin priority threshold
        }

        public async Task<bool> CanManageRoleAsync(Guid userId, Guid roleId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            var targetRole = await _context.Roles.FindAsync(roleId);

            if (user == null || targetRole == null) return false;

            // Admins can manage all roles except they cannot delete the admin role
            if (await IsAdminAsync(userId))
            {
                if (targetRole.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !targetRole.CanBeDeleted)
                    return false; // Admin role cannot be deleted
                return true;
            }

            // Users can only manage roles with lower priority than their own
            if (user.Role != null && user.Role.Priority > targetRole.Priority)
                return await HasPermissionAsync(userId, PermissionConstants.ADMIN_MANAGE_ROLES);

            return false;
        }

        public async Task<Role?> GetUserRoleAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            return user?.Role;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid assignedByUserId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            var newRole = await _context.Roles.FindAsync(roleId);
            var assignedBy = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == assignedByUserId);

            if (user == null || newRole == null || assignedBy == null) return false;

            // Check if the assigner can manage this role
            if (!await CanManageRoleAsync(assignedByUserId, roleId)) return false;

            // Special rule: Only admins can assign admin role
            if (newRole.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !await IsAdminAsync(assignedByUserId))
                return false;

            // Assign the role
            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;

            // Clear permission cache for this user
            _userPermissionCache.Remove(userId);
            _cacheTimestamps.Remove(userId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateRoleHierarchyAsync(Guid? parentRoleId, int priority)
        {
            if (parentRoleId == null) return true;

            var parentRole = await _context.Roles.FindAsync(parentRoleId);
            if (parentRole == null) return false;

            // Child role must have lower priority than parent
            return priority < parentRole.Priority;
        }

        #endregion

        #region User Permission Overrides

        public async Task<bool> GrantUserPermissionAsync(Guid userId, string permission, Guid grantedByUserId, string? reason = null, DateTime? expiresAt = null)
        {
            var user = await _context.Users.FindAsync(userId);
            var grantedByUser = await _context.Users.FindAsync(grantedByUserId);
            var permissionEntity = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permission);

            if (user == null || grantedByUser == null || permissionEntity == null) return false;

            // Only admins can grant permissions, or users with permission management rights
            if (!await IsAdminAsync(grantedByUserId) && !await HasPermissionAsync(grantedByUserId, PermissionConstants.ADMIN_MANAGE_PERMISSIONS))
                return false;

            // Remove existing override if any
            var existingOverride = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionEntity.Id);

            if (existingOverride != null)
                _context.UserPermissions.Remove(existingOverride);

            // Create new override
            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permissionEntity.Id,
                IsGranted = true,
                ExpiresAt = expiresAt,
                Reason = reason,
                GrantedByUserId = grantedByUserId
            };

            _context.UserPermissions.Add(userPermission);

            // Clear cache
            _userPermissionCache.Remove(userId);
            _cacheTimestamps.Remove(userId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeUserPermissionAsync(Guid userId, string permission)
        {
            var permissionEntity = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permission);
            if (permissionEntity == null) return false;

            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionEntity.Id);

            if (userPermission != null)
            {
                _context.UserPermissions.Remove(userPermission);
                
                // Clear cache
                _userPermissionCache.Remove(userId);
                _cacheTimestamps.Remove(userId);

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<List<UserPermission>> GetUserPermissionOverridesAsync(Guid userId)
        {
            return await _context.UserPermissions
                .Include(up => up.Permission)
                .Include(up => up.GrantedByUser)
                .Where(up => up.UserId == userId)
                .ToListAsync();
        }

        #endregion

        #region System Operations

        public async Task InitializeDefaultPermissionsAsync()
        {
            var permissionsToCreate = new List<(string name, string description, string category)>();

            // Add all permissions from PermissionConstants
            var permissionType = typeof(PermissionConstants);
            var fields = permissionType.GetFields().Where(f => f.IsLiteral && !f.IsInitOnly);

            foreach (var field in fields)
            {
                var permissionName = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(permissionName))
                {
                    var category = GetPermissionCategory(permissionName);
                    var description = GeneratePermissionDescription(permissionName);
                    permissionsToCreate.Add((permissionName, description, category));
                }
            }

            foreach (var (name, description, category) in permissionsToCreate)
            {
                var exists = await _context.Permissions.AnyAsync(p => p.Name == name);
                if (!exists)
                {
                    _context.Permissions.Add(new Permission
                    {
                        Name = name,
                        Description = description,
                        Category = category,
                        IsSystemPermission = true,
                        IsActive = true
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task InitializeDefaultRolesAsync()
        {
            // Create default roles
            var defaultRoles = new[]
            {
                new Role { Name = "User", Description = "Standard user role", Priority = 100, IsSystemRole = true, IsDefault = true, CanBeDeleted = false },
                new Role { Name = "Moderator", Description = "Content moderation role", Priority = 500, IsSystemRole = true, CanBeDeleted = true },
                new Role { Name = "Admin", Description = "System administrator role", Priority = 1000, IsSystemRole = true, CanBeDeleted = false }
            };

            foreach (var role in defaultRoles)
            {
                var exists = await _context.Roles.AnyAsync(r => r.Name == role.Name);
                if (!exists)
                {
                    _context.Roles.Add(role);
                }
            }

            await _context.SaveChangesAsync();

            // Assign permissions to roles
            await AssignDefaultPermissionsToRolesAsync();
        }

        public async Task<bool> ValidateUserCanPerformActionOnResourceAsync(Guid userId, string action, string resourceType, Guid? resourceOwnerId = null)
        {
            // Check if user has general permission for this action
            if (!await HasPermissionAsync(userId, action))
                return false;

            // If it's an "own" action, check ownership
            if (action.Contains(".own") && resourceOwnerId.HasValue)
            {
                return userId == resourceOwnerId.Value;
            }

            // If it's an "any" action, user already passed the permission check
            if (action.Contains(".any"))
                return true;

            // For other actions, permission check was sufficient
            return true;
        }

        #endregion

        #region Private Helper Methods

        private async Task<bool> HasInheritedPermissionAsync(Guid roleId, string permission)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role?.ParentRoleId == null) return false;

            var parentRolePermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp => rp.RoleId == role.ParentRoleId && 
                                          rp.Permission.Name == permission &&
                                          rp.IsGranted);

            if (parentRolePermission != null) return true;

            // Check recursively up the hierarchy
            return await HasInheritedPermissionAsync(role.ParentRoleId.Value, permission);
        }

        private async Task<List<string>> GetRolePermissionsRecursiveAsync(Guid roleId)
        {
            var permissions = new HashSet<string>();
            
            // Get direct permissions
            var directPermissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && rp.IsGranted)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            foreach (var perm in directPermissions)
                permissions.Add(perm);

            // Get inherited permissions
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role?.ParentRoleId != null)
            {
                var inheritedPermissions = await GetRolePermissionsRecursiveAsync(role.ParentRoleId.Value);
                foreach (var perm in inheritedPermissions)
                    permissions.Add(perm);
            }

            return permissions.ToList();
        }

        private async Task AssignDefaultPermissionsToRolesAsync()
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var moderatorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Moderator");
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            if (userRole != null)
                await AssignPermissionsToRoleAsync(userRole.Id, GetFlatPermissionList(PermissionConstants.DefaultRoles.USER));

            if (moderatorRole != null)
            {
                await AssignPermissionsToRoleAsync(moderatorRole.Id, GetFlatPermissionList(PermissionConstants.DefaultRoles.USER));
                await AssignPermissionsToRoleAsync(moderatorRole.Id, GetFlatPermissionList(PermissionConstants.DefaultRoles.MODERATOR));
            }

            if (adminRole != null)
            {
                await AssignPermissionsToRoleAsync(adminRole.Id, GetFlatPermissionList(PermissionConstants.DefaultRoles.USER));
                await AssignPermissionsToRoleAsync(adminRole.Id, GetFlatPermissionList(PermissionConstants.DefaultRoles.MODERATOR));
                await AssignPermissionsToRoleAsync(adminRole.Id, GetFlatPermissionList(PermissionConstants.DefaultRoles.ADMIN));
            }
        }

        private async Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissions)
        {
            foreach (var permissionName in permissions)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission != null)
                {
                    var exists = await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);
                    if (!exists)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = roleId,
                            PermissionId = permission.Id,
                            IsGranted = true
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private List<string> GetFlatPermissionList(Dictionary<string, string[]> permissionsByCategory)
        {
            return permissionsByCategory.Values.SelectMany(x => x).ToList();
        }

        private string GetPermissionCategory(string permissionName)
        {
            return permissionName.Split('.')[0] switch
            {
                "admin" => PermissionConstants.Categories.ADMINISTRATION,
                "user" => PermissionConstants.Categories.USER_MANAGEMENT,
                "model" => PermissionConstants.Categories.MODEL_MANAGEMENT,
                "moderation" => PermissionConstants.Categories.MODERATION,
                "collection" => PermissionConstants.Categories.COLLECTIONS,
                "comment" => PermissionConstants.Categories.COMMENTS,
                "report" => PermissionConstants.Categories.REPORTS,
                "plugin" => PermissionConstants.Categories.PLUGINS,
                "api" => PermissionConstants.Categories.API_ACCESS,
                "storage" => PermissionConstants.Categories.STORAGE,
                _ => "Other"
            };
        }

        private string GeneratePermissionDescription(string permissionName)
        {
            var parts = permissionName.Split('.');
            if (parts.Length >= 3)
            {
                var action = parts[1];
                var resource = parts[2].Replace("_", " ");
                return $"Allows user to {action} {resource}";
            }
            return $"Permission: {permissionName}";
        }

        #endregion
    }
} 