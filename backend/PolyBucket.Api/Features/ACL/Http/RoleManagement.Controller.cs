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

namespace PolyBucket.Api.Features.ACL.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/roles")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
    public class RoleManagementController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public RoleManagementController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Get all roles with their permissions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), 200)]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesAsync();
            var roleDtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var permissions = await GetRolePermissionsAsync(role.Id);
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
                    Permissions = permissions
                });
            }

            return Ok(roleDtos);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RoleDto>> GetRole(Guid id)
        {
            var roles = await _permissionService.GetAllRolesAsync();
            var role = roles.Find(r => r.Id == id);
            
            if (role == null)
                return NotFound("Role not found");

            var permissions = await GetRolePermissionsAsync(role.Id);
            
            var roleDto = new RoleDto
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
                Permissions = permissions
            };

            return Ok(roleDto);
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            // Validate hierarchy
            if (!await _permissionService.ValidateRoleHierarchyAsync(request.ParentRoleId, request.Priority))
                return BadRequest("Invalid role hierarchy: child role must have lower priority than parent");

            // Only admins can create system roles or roles with admin-level priority
            var isAdmin = await _permissionService.IsAdminAsync(userId);
            if (!isAdmin && (request.IsSystemRole || request.Priority >= 1000))
                return Forbid("Only administrators can create system roles or high-priority roles");

            try
            {
                var role = await CreateRoleAsync(request);
                var permissions = await GetRolePermissionsAsync(role.Id);
                
                var roleDto = new RoleDto
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
                    Permissions = permissions
                };

                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create role: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RoleDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!await _permissionService.CanManageRoleAsync(userId, id))
                return Forbid("Insufficient privileges to manage this role");

            try
            {
                var updatedRole = await UpdateRoleAsync(id, request);
                if (updatedRole == null)
                    return NotFound("Role not found");

                var permissions = await GetRolePermissionsAsync(updatedRole.Id);
                
                var roleDto = new RoleDto
                {
                    Id = updatedRole.Id,
                    Name = updatedRole.Name,
                    Description = updatedRole.Description,
                    Priority = updatedRole.Priority,
                    IsSystemRole = updatedRole.IsSystemRole,
                    IsDefault = updatedRole.IsDefault,
                    CanBeDeleted = updatedRole.CanBeDeleted,
                    IsActive = updatedRole.IsActive,
                    ParentRoleId = updatedRole.ParentRoleId,
                    Permissions = permissions
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update role: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!await _permissionService.CanManageRoleAsync(userId, id))
                return Forbid("Insufficient privileges to manage this role");

            try
            {
                var success = await DeleteRoleAsync(id);
                if (!success)
                    return NotFound("Role not found or cannot be deleted");

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to delete role: {ex.Message}");
            }
        }

        /// <summary>
        /// Assign permissions to a role
        /// </summary>
        [HttpPost("{id}/permissions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> AssignPermissionsToRole(Guid id, [FromBody] AssignPermissionsRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!await _permissionService.CanManageRoleAsync(userId, id))
                return Forbid("Insufficient privileges to manage this role");

            try
            {
                await AssignPermissionsToRoleAsync(id, request.PermissionNames);
                return Ok(new { message = "Permissions assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to assign permissions: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private async Task<List<string>> GetRolePermissionsAsync(Guid roleId)
        {
            // This would typically be implemented in the permission service
            // For now, return empty list as placeholder
            return new List<string>();
        }

        private async Task<Role> CreateRoleAsync(CreateRoleRequest request)
        {
            // Implementation would create role in database
            // Placeholder implementation
            throw new NotImplementedException("Role creation not yet implemented");
        }

        private async Task<Role?> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
        {
            // Implementation would update role in database
            // Placeholder implementation
            throw new NotImplementedException("Role update not yet implemented");
        }

        private async Task<bool> DeleteRoleAsync(Guid id)
        {
            // Implementation would delete role from database
            // Placeholder implementation
            throw new NotImplementedException("Role deletion not yet implemented");
        }

        private async Task AssignPermissionsToRoleAsync(Guid roleId, List<string> permissionNames)
        {
            // Implementation would assign permissions to role
            // Placeholder implementation
            throw new NotImplementedException("Permission assignment not yet implemented");
        }

        #endregion
    }

    #region DTOs

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
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class CreateRoleRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 999)]
        public int Priority { get; set; } = 100;

        public bool IsSystemRole { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public Guid? ParentRoleId { get; set; }
        public List<string> InitialPermissions { get; set; } = new List<string>();
    }

    public class UpdateRoleRequest
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 999)]
        public int? Priority { get; set; }

        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public Guid? ParentRoleId { get; set; }
    }

    public class AssignPermissionsRequest
    {
        [Required]
        public List<string> PermissionNames { get; set; } = new List<string>();
    }

    #endregion
} 