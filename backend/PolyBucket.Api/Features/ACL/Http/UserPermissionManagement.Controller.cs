using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace PolyBucket.Api.Features.ACL.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/user-permissions")]
    public class UserPermissionManagementController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public UserPermissionManagementController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Get user's effective permissions (role + overrides)
        /// </summary>
        [HttpGet("{userId}/permissions")]
        [RequirePermission(PermissionConstants.ADMIN_VIEW_USER_DETAILS)]
        [ProducesResponseType(typeof(UserPermissionsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions(Guid userId)
        {
            var userRole = await _permissionService.GetUserRoleAsync(userId);
            if (userRole == null)
                return NotFound("User not found");

            var effectivePermissions = await _permissionService.GetUserPermissionsAsync(userId);
            var permissionOverrides = await _permissionService.GetUserPermissionOverridesAsync(userId);

            var dto = new UserPermissionsDto
            {
                UserId = userId,
                Role = new UserRoleDto
                {
                    Id = userRole.Id,
                    Name = userRole.Name,
                    Priority = userRole.Priority
                },
                EffectivePermissions = effectivePermissions,
                PermissionOverrides = permissionOverrides.Select(po => new PermissionOverrideDto
                {
                    Id = po.Id,
                    PermissionName = po.Permission.Name,
                    IsGranted = po.IsGranted,
                    Reason = po.Reason,
                    ExpiresAt = po.ExpiresAt,
                    GrantedByUsername = po.GrantedByUser?.Username,
                    CreatedAt = po.CreatedAt
                }).ToList()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("{userId}/role")]
        [RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> AssignUserRole(Guid userId, [FromBody] AssignRoleRequest request)
        {
            var assignedByUserClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(assignedByUserClaim, out var assignedByUserId))
                return Unauthorized();

            // Check if assigner can assign this role
            if (!await _permissionService.CanManageRoleAsync(assignedByUserId, request.RoleId))
                return Forbid("Insufficient privileges to assign this role");

            // Prevent non-admins from promoting users to admin
            var targetRole = (await _permissionService.GetAllRolesAsync()).FirstOrDefault(r => r.Id == request.RoleId);
            if (targetRole?.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (!await _permissionService.IsAdminAsync(assignedByUserId))
                    return Forbid("Only administrators can assign the admin role");
            }

            try
            {
                var success = await _permissionService.AssignRoleAsync(userId, request.RoleId, assignedByUserId);
                if (!success)
                    return BadRequest("Failed to assign role");

                return Ok(new { message = "Role assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to assign role: {ex.Message}");
            }
        }

        /// <summary>
        /// Grant specific permission to user (override)
        /// </summary>
        [HttpPost("{userId}/permissions/grant")]
        [RequirePermission(PermissionConstants.ADMIN_MANAGE_PERMISSIONS)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> GrantUserPermission(Guid userId, [FromBody] GrantPermissionRequest request)
        {
            var grantedByUserClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(grantedByUserClaim, out var grantedByUserId))
                return Unauthorized();

            // Prevent granting admin permissions to non-admins
            if (request.Permission.StartsWith("admin.") && !await _permissionService.IsAdminAsync(grantedByUserId))
                return Forbid("Only administrators can grant admin permissions");

            try
            {
                var success = await _permissionService.GrantUserPermissionAsync(
                    userId, 
                    request.Permission, 
                    grantedByUserId, 
                    request.Reason, 
                    request.ExpiresAt);

                if (!success)
                    return BadRequest("Failed to grant permission");

                return Ok(new { message = "Permission granted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to grant permission: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke specific permission from user
        /// </summary>
        [HttpDelete("{userId}/permissions/{permission}")]
        [RequirePermission(PermissionConstants.ADMIN_MANAGE_PERMISSIONS)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> RevokeUserPermission(Guid userId, string permission)
        {
            var revokedByUserClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(revokedByUserClaim, out var revokedByUserId))
                return Unauthorized();

            // Prevent revoking admin permissions unless done by admin
            if (permission.StartsWith("admin.") && !await _permissionService.IsAdminAsync(revokedByUserId))
                return Forbid("Only administrators can revoke admin permissions");

            try
            {
                var success = await _permissionService.RevokeUserPermissionAsync(userId, permission);
                if (!success)
                    return BadRequest("Failed to revoke permission");

                return Ok(new { message = "Permission revoked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to revoke permission: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if user can moderate another user
        /// </summary>
        [HttpGet("{moderatorId}/can-moderate/{targetUserId}")]
        [RequirePermission(PermissionConstants.ADMIN_VIEW_USER_DETAILS)]
        [ProducesResponseType(typeof(ModerationCheckDto), 200)]
        public async Task<ActionResult<ModerationCheckDto>> CanUserModerateUser(Guid moderatorId, Guid targetUserId)
        {
            var canModerate = await _permissionService.CanUserModerateUserAsync(moderatorId, targetUserId);
            var moderatorRole = await _permissionService.GetUserRoleAsync(moderatorId);
            var targetRole = await _permissionService.GetUserRoleAsync(targetUserId);

            var dto = new ModerationCheckDto
            {
                CanModerate = canModerate,
                ModeratorRole = moderatorRole?.Name ?? "Unknown",
                TargetRole = targetRole?.Name ?? "Unknown",
                Reason = canModerate 
                    ? "User has sufficient moderation privileges" 
                    : "User lacks moderation privileges or target user has equal/higher role"
            };

            return Ok(dto);
        }

        /// <summary>
        /// Get all available permissions
        /// </summary>
        [HttpGet("permissions")]
        [RequirePermission(PermissionConstants.ADMIN_VIEW_USER_DETAILS)]
        [ProducesResponseType(typeof(List<PermissionDto>), 200)]
        public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions()
        {
            // This would typically come from the permission service
            // For now, return permissions from constants
            var permissions = new List<PermissionDto>();

            var permissionType = typeof(PermissionConstants);
            var fields = permissionType.GetFields().Where(f => f.IsLiteral && !f.IsInitOnly);

            foreach (var field in fields)
            {
                var permissionName = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(permissionName))
                {
                    permissions.Add(new PermissionDto
                    {
                        Name = permissionName,
                        Category = GetPermissionCategory(permissionName),
                        Description = GeneratePermissionDescription(permissionName)
                    });
                }
            }

            return Ok(permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList());
        }

        #region Private Helper Methods

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

    #region DTOs

    public class UserPermissionsDto
    {
        public Guid UserId { get; set; }
        public UserRoleDto Role { get; set; } = null!;
        public List<string> EffectivePermissions { get; set; } = new List<string>();
        public List<PermissionOverrideDto> PermissionOverrides { get; set; } = new List<PermissionOverrideDto>();
    }

    public class UserRoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class PermissionOverrideDto
    {
        public Guid Id { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
        public string? Reason { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? GrantedByUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PermissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ModerationCheckDto
    {
        public bool CanModerate { get; set; }
        public string ModeratorRole { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class AssignRoleRequest
    {
        [Required]
        public Guid RoleId { get; set; }
    }

    public class GrantPermissionRequest
    {
        [Required]
        public string Permission { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    #endregion
} 