using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;

namespace PolyBucket.Api.Features.ACL.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/roles")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
    public class RoleManagementController : ControllerBase
    {
        private readonly IRoleManagementService _roleManagementService;
        private readonly IPermissionService _permissionService;

        public RoleManagementController(IRoleManagementService roleManagementService, IPermissionService permissionService)
        {
            _roleManagementService = roleManagementService;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Get all roles with their permissions (paginated)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedRolesResponse), 200)]
        public async Task<ActionResult<PaginatedRolesResponse>> GetAllRoles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "priority",
            [FromQuery] bool sortDescending = true)
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var roles = await _roleManagementService.GetAllRolesAsync();
            var roleDtos = new List<RoleDto>();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                roles = roles.Where(r => 
                    r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Apply sorting
            roles = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? roles.OrderByDescending(r => r.Name).ToList() : roles.OrderBy(r => r.Name).ToList(),
                "priority" => sortDescending ? roles.OrderByDescending(r => r.Priority).ToList() : roles.OrderBy(r => r.Priority).ToList(),
                "users" => sortDescending ? roles.OrderByDescending(r => r.Users.Count).ToList() : roles.OrderBy(r => r.Users.Count).ToList(),
                _ => sortDescending ? roles.OrderByDescending(r => r.Priority).ToList() : roles.OrderBy(r => r.Priority).ToList()
            };

            // Calculate pagination
            var totalCount = roles.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;
            var pagedRoles = roles.Skip(skip).Take(pageSize).ToList();

            // Build DTOs for the current page
            foreach (var role in pagedRoles)
            {
                var permissions = await _roleManagementService.GetRolePermissionsAsync(role.Id);
                var userCount = await _roleManagementService.GetUserCountAsync(role.Id);
                
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

            var response = new PaginatedRolesResponse
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

            return Ok(response);
        }

        /// <summary>
        /// Get all roles without pagination (for backward compatibility)
        /// </summary>
        [HttpGet("unpaginated")]
        [ProducesResponseType(typeof(List<RoleDto>), 200)]
        public async Task<ActionResult<List<RoleDto>>> GetAllRolesUnpaginated()
        {
            var roles = await _roleManagementService.GetAllRolesAsync();
            var roleDtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var permissions = await _roleManagementService.GetRolePermissionsAsync(role.Id);
                var userCount = await _roleManagementService.GetUserCountAsync(role.Id);
                
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
            var role = await _roleManagementService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound("Role not found");

            var permissions = await _roleManagementService.GetRolePermissionsAsync(role.Id);
            var userCount = await _roleManagementService.GetUserCountAsync(role.Id);
            
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
                Permissions = permissions,
                UserCount = userCount,
                Color = role.Color
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

            try
            {
                var role = new Role
                {
                    Name = request.Name,
                    Description = request.Description,
                    Priority = request.Priority,
                    IsSystemRole = request.IsSystemRole,
                    IsDefault = request.IsDefault,
                    ParentRoleId = request.ParentRoleId,
                    IsActive = true,
                    CanBeDeleted = !request.IsSystemRole,
                    Color = request.Color
                };

                var createdRole = await _roleManagementService.CreateRoleAsync(role, request.InitialPermissions);
                var permissions = await _roleManagementService.GetRolePermissionsAsync(createdRole.Id);
                var userCount = await _roleManagementService.GetUserCountAsync(createdRole.Id);
                
                var roleDto = new RoleDto
                {
                    Id = createdRole.Id,
                    Name = createdRole.Name,
                    Description = createdRole.Description,
                    Priority = createdRole.Priority,
                    IsSystemRole = createdRole.IsSystemRole,
                    IsDefault = createdRole.IsDefault,
                    CanBeDeleted = createdRole.CanBeDeleted,
                    IsActive = createdRole.IsActive,
                    ParentRoleId = createdRole.ParentRoleId,
                    Permissions = permissions,
                    UserCount = userCount,
                    Color = createdRole.Color
                };

                return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, roleDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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

            if (!await _roleManagementService.CanManageRoleAsync(userId, id))
                return Forbid("Insufficient privileges to manage this role");

            try
            {
                var existingRole = await _roleManagementService.GetRoleByIdAsync(id);
                if (existingRole == null)
                    return NotFound("Role not found");

                var updatedRole = await _roleManagementService.UpdateRoleAsync(id, request);
                var permissions = await _roleManagementService.GetRolePermissionsAsync(updatedRole.Id);
                var userCount = await _roleManagementService.GetUserCountAsync(updatedRole.Id);
                
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
                    Permissions = permissions,
                    UserCount = userCount,
                    Color = updatedRole.Color
                };

                return Ok(roleDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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

            if (!await _roleManagementService.CanDeleteRoleAsync(userId, id))
                return Forbid("Insufficient privileges to delete this role");

            try
            {
                var success = await _roleManagementService.DeleteRoleAsync(id);
                if (!success)
                    return NotFound("Role not found or cannot be deleted");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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

            if (!await _roleManagementService.CanManageRoleAsync(userId, id))
                return Forbid("Insufficient privileges to manage this role");

            try
            {
                await _roleManagementService.AssignPermissionsToRoleAsync(id, request.PermissionNames);
                return Ok(new { message = "Permissions assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to assign permissions: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove permissions from a role
        /// </summary>
        [HttpDelete("{id}/permissions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RemovePermissionsFromRole(Guid id, [FromBody] RemovePermissionsRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!await _roleManagementService.CanManageRoleAsync(userId, id))
                return Forbid("Insufficient privileges to manage this role");

            try
            {
                await _roleManagementService.RemovePermissionsFromRoleAsync(id, request.PermissionNames);
                return Ok(new { message = "Permissions removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to remove permissions: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all available permissions
        /// </summary>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(List<PermissionDto>), 200)]
        public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            var permissionDtos = new List<PermissionDto>();

            foreach (var permission in permissions)
            {
                permissionDtos.Add(new PermissionDto
                {
                    Name = permission.Name,
                    Category = GetPermissionCategory(permission.Name),
                    Description = GeneratePermissionDescription(permission.Name)
                });
            }

            return Ok(permissionDtos.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList());
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
            if (parts.Length < 2) return permissionName;

            var action = parts[1].Replace('_', ' ');
            var resource = parts[0].Replace('_', ' ');

            return $"{action} {resource}";
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
        [JsonPropertyName("userCount")]
        public int UserCount { get; set; } = 1;
        public string Color { get; set; } = "#3B82F6";
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
        
        [StringLength(7, MinimumLength = 7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #3B82F6)")]
        public string Color { get; set; } = "#3B82F6";
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
        
        [StringLength(7, MinimumLength = 7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #3B82F6)")]
        public string? Color { get; set; }
    }

    public class AssignPermissionsRequest
    {
        [Required]
        public List<string> PermissionNames { get; set; } = new List<string>();
    }

    public class RemovePermissionsRequest
    {
        [Required]
        public List<string> PermissionNames { get; set; } = new List<string>();
    }

    public class PermissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PaginatedRolesResponse
    {
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
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

    #endregion
} 