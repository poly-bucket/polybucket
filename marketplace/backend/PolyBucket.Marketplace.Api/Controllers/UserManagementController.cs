using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;
using System.Security.Claims;

namespace PolyBucket.Marketplace.Api.Controllers
{
    /// <summary>
    /// Controller for user management and role-based access control
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IUserManagementService userManagementService,
            ILogger<UserManagementController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (admin only)
        /// </summary>
        [HttpGet("users")]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManagementService.GetUsersByStatusAsync("active");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new { error = "Failed to get users" });
            }
        }

        /// <summary>
        /// Get users by role (admin/moderator only)
        /// </summary>
        [HttpGet("users/role/{role}")]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await _userManagementService.GetUsersByRoleAsync(role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role");
                return StatusCode(500, new { error = "Failed to get users by role" });
            }
        }

        /// <summary>
        /// Get users by status (admin only)
        /// </summary>
        [HttpGet("users/status/{status}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetUsersByStatus(string status)
        {
            try
            {
                var users = await _userManagementService.GetUsersByStatusAsync(status);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by status");
                return StatusCode(500, new { error = "Failed to get users by status" });
            }
        }

        /// <summary>
        /// Get user details with roles and permissions
        /// </summary>
        [HttpGet("users/{userId}")]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            try
            {
                var user = await _userManagementService.GetUserWithRolesAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details");
                return StatusCode(500, new { error = "Failed to get user details" });
            }
        }

        /// <summary>
        /// Assign role to user (admin only)
        /// </summary>
        [HttpPost("users/{userId}/roles")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
        {
            try
            {
                if (request.UserId != userId)
                {
                    return BadRequest(new { error = "User ID mismatch" });
                }

                var result = await _userManagementService.AssignRoleAsync(userId, request.Role, request.IsPrimaryRole);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user");
                return StatusCode(500, new { error = "Failed to assign role" });
            }
        }

        /// <summary>
        /// Remove role from user (admin only)
        /// </summary>
        [HttpDelete("users/{userId}/roles/{role}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            try
            {
                var result = await _userManagementService.RemoveRoleAsync(userId, role);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user");
                return StatusCode(500, new { error = "Failed to remove role" });
            }
        }

        /// <summary>
        /// Update user status (admin only)
        /// </summary>
        [HttpPut("users/{userId}/status")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                if (request.UserId != userId)
                {
                    return BadRequest(new { error = "User ID mismatch" });
                }

                var result = await _userManagementService.UpdateUserStatusAsync(userId, request.Status, request.Reason);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status");
                return StatusCode(500, new { error = "Failed to update user status" });
            }
        }

        /// <summary>
        /// Promote user to developer (admin/moderator only)
        /// </summary>
        [HttpPost("users/{userId}/promote/developer")]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> PromoteToDeveloper(string userId)
        {
            try
            {
                var result = await _userManagementService.PromoteToDeveloperAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to developer");
                return StatusCode(500, new { error = "Failed to promote user" });
            }
        }

        /// <summary>
        /// Promote user to moderator (admin only)
        /// </summary>
        [HttpPost("users/{userId}/promote/moderator")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> PromoteToModerator(string userId)
        {
            try
            {
                var result = await _userManagementService.PromoteToModeratorAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to moderator");
                return StatusCode(500, new { error = "Failed to promote user" });
            }
        }

        /// <summary>
        /// Demote user to basic user (admin only)
        /// </summary>
        [HttpPost("users/{userId}/demote")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DemoteUser(string userId)
        {
            try
            {
                var result = await _userManagementService.DemoteUserAsync(userId);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error demoting user");
                return StatusCode(500, new { error = "Failed to demote user" });
            }
        }

        /// <summary>
        /// Add custom permission to user (admin only)
        /// </summary>
        [HttpPost("users/{userId}/permissions")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> AddCustomPermission(string userId, [FromBody] AddPermissionRequest request)
        {
            try
            {
                var result = await _userManagementService.AddCustomPermissionAsync(userId, request.Permission);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding custom permission to user");
                return StatusCode(500, new { error = "Failed to add custom permission" });
            }
        }

        /// <summary>
        /// Remove custom permission from user (admin only)
        /// </summary>
        [HttpDelete("users/{userId}/permissions/{permission}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> RemoveCustomPermission(string userId, string permission)
        {
            try
            {
                var result = await _userManagementService.RemoveCustomPermissionAsync(userId, permission);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing custom permission from user");
                return StatusCode(500, new { error = "Failed to remove custom permission" });
            }
        }

        /// <summary>
        /// Get current user's permissions
        /// </summary>
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var permissions = await _userManagementService.GetUserPermissionsAsync(userId);
                return Ok(new { permissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions");
                return StatusCode(500, new { error = "Failed to get permissions" });
            }
        }

        /// <summary>
        /// Get current user's roles
        /// </summary>
        [HttpGet("me/roles")]
        public async Task<IActionResult> GetMyRoles()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var roles = await _userManagementService.GetUserRolesAsync(userId);
                return Ok(new { roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return StatusCode(500, new { error = "Failed to get roles" });
            }
        }

        /// <summary>
        /// Check if current user has permission
        /// </summary>
        [HttpGet("me/has-permission/{permission}")]
        public async Task<IActionResult> HasPermission(string permission)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var hasPermission = await _userManagementService.UserHasPermissionAsync(userId, permission);
                return Ok(new { hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission");
                return StatusCode(500, new { error = "Failed to check permission" });
            }
        }

        /// <summary>
        /// Check if current user is in role
        /// </summary>
        [HttpGet("me/in-role/{role}")]
        public async Task<IActionResult> IsInRole(string role)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var isInRole = await _userManagementService.UserIsInRoleAsync(userId, role);
                return Ok(new { isInRole });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user role");
                return StatusCode(500, new { error = "Failed to check role" });
            }
        }
    }

    /// <summary>
    /// Request model for adding permission
    /// </summary>
    public class AddPermissionRequest
    {
        public string Permission { get; set; } = string.Empty;
    }
}
