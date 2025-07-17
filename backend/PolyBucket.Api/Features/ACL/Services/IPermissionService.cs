using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Services
{
    public interface IPermissionService
    {
        // Permission Checking
        Task<bool> HasPermissionAsync(Guid userId, string permission);
        Task<bool> HasAnyPermissionAsync(Guid userId, params string[] permissions);
        Task<bool> HasAllPermissionsAsync(Guid userId, params string[] permissions);
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
        Task<bool> CanUserModerateUserAsync(Guid moderatorId, Guid targetUserId);
        
        // Role Management
        Task<bool> IsAdminAsync(Guid userId);
        Task<bool> CanManageRoleAsync(Guid userId, Guid roleId);
        Task<Role?> GetUserRoleAsync(Guid userId);
        Task<List<Role>> GetAllRolesAsync();
        Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid assignedByUserId);
        Task<bool> ValidateRoleHierarchyAsync(Guid? parentRoleId, int priority);
        
        // User Permission Overrides
        Task<bool> GrantUserPermissionAsync(Guid userId, string permission, Guid grantedByUserId, string? reason = null, DateTime? expiresAt = null);
        Task<bool> RevokeUserPermissionAsync(Guid userId, string permission);
        Task<List<UserPermission>> GetUserPermissionOverridesAsync(Guid userId);
        
        // System Operations
        Task InitializeDefaultPermissionsAsync();
        Task InitializeDefaultRolesAsync();
        Task<bool> ValidateUserCanPerformActionOnResourceAsync(Guid userId, string action, string resourceType, Guid? resourceOwnerId = null);
    }
} 