using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services
{
    /// <summary>
    /// Service for user management and role-based access control
    /// </summary>
    public interface IUserManagementService
    {
        Task<UserManagementResponse> AssignRoleAsync(string userId, string role, bool isPrimaryRole = false);
        Task<UserManagementResponse> RemoveRoleAsync(string userId, string role);
        Task<UserManagementResponse> UpdateUserStatusAsync(string userId, string status, string? reason = null);
        Task<UserManagementResponse> AddCustomPermissionAsync(string userId, string permission);
        Task<UserManagementResponse> RemoveCustomPermissionAsync(string userId, string permission);
        Task<List<UserProfile>> GetUsersByRoleAsync(string role);
        Task<List<UserProfile>> GetUsersByStatusAsync(string status);
        Task<UserProfile?> GetUserWithRolesAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permission);
        Task<bool> UserIsInRoleAsync(string userId, string role);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<UserManagementResponse> PromoteToDeveloperAsync(string userId);
        Task<UserManagementResponse> PromoteToModeratorAsync(string userId);
        Task<UserManagementResponse> DemoteUserAsync(string userId);
    }

    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<MarketplaceUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MarketplaceDbContext _context;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<MarketplaceUser> userManager,
            RoleManager<IdentityRole> roleManager,
            MarketplaceDbContext context,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        public async Task<UserManagementResponse> AssignRoleAsync(string userId, string role, bool isPrimaryRole = false)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "User not found"
                    };
                }

                // Validate role
                if (!IsValidRole(role))
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "Invalid role"
                    };
                }

                // Ensure role exists in Identity
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // Add role to Identity
                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Update primary role if specified
                if (isPrimaryRole)
                {
                    user.PrimaryRole = role;
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("Assigned role {Role} to user {UserId}", role, userId);

                return new UserManagementResponse
                {
                    Success = true,
                    Message = $"Successfully assigned role {role} to user",
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user");
                return new UserManagementResponse
                {
                    Success = false,
                    Error = "Failed to assign role"
                };
            }
        }

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        public async Task<UserManagementResponse> RemoveRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "User not found"
                    };
                }

                // Remove role from Identity
                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Update primary role if it was the removed role
                if (user.PrimaryRole == role)
                {
                    user.PrimaryRole = UserRoles.User; // Default to User role
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("Removed role {Role} from user {UserId}", role, userId);

                return new UserManagementResponse
                {
                    Success = true,
                    Message = $"Successfully removed role {role} from user",
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user");
                return new UserManagementResponse
                {
                    Success = false,
                    Error = "Failed to remove role"
                };
            }
        }

        /// <summary>
        /// Update user status
        /// </summary>
        public async Task<UserManagementResponse> UpdateUserStatusAsync(string userId, string status, string? reason = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "User not found"
                    };
                }

                // Validate status
                if (!IsValidStatus(status))
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "Invalid status"
                    };
                }

                user.Status = status;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Updated user {UserId} status to {Status}. Reason: {Reason}", 
                    userId, status, reason ?? "No reason provided");

                return new UserManagementResponse
                {
                    Success = true,
                    Message = $"Successfully updated user status to {status}",
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status");
                return new UserManagementResponse
                {
                    Success = false,
                    Error = "Failed to update user status"
                };
            }
        }

        /// <summary>
        /// Add custom permission to user
        /// </summary>
        public async Task<UserManagementResponse> AddCustomPermissionAsync(string userId, string permission)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "User not found"
                    };
                }

                var permissions = user.CustomPermissions?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()).ToList() ?? new List<string>();

                if (!permissions.Contains(permission))
                {
                    permissions.Add(permission);
                    user.CustomPermissions = string.Join(",", permissions);
                    await _userManager.UpdateAsync(user);
                }

                return new UserManagementResponse
                {
                    Success = true,
                    Message = $"Successfully added permission {permission} to user",
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding custom permission to user");
                return new UserManagementResponse
                {
                    Success = false,
                    Error = "Failed to add custom permission"
                };
            }
        }

        /// <summary>
        /// Remove custom permission from user
        /// </summary>
        public async Task<UserManagementResponse> RemoveCustomPermissionAsync(string userId, string permission)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "User not found"
                    };
                }

                var permissions = user.CustomPermissions?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()).ToList() ?? new List<string>();

                permissions.Remove(permission);
                user.CustomPermissions = permissions.Any() ? string.Join(",", permissions) : null;
                await _userManager.UpdateAsync(user);

                return new UserManagementResponse
                {
                    Success = true,
                    Message = $"Successfully removed permission {permission} from user",
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing custom permission from user");
                return new UserManagementResponse
                {
                    Success = false,
                    Error = "Failed to remove custom permission"
                };
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        public async Task<List<UserProfile>> GetUsersByRoleAsync(string role)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(role);
                var profiles = new List<UserProfile>();

                foreach (var user in users)
                {
                    profiles.Add(await MapToUserProfileAsync(user));
                }

                return profiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role");
                return new List<UserProfile>();
            }
        }

        /// <summary>
        /// Get users by status
        /// </summary>
        public async Task<List<UserProfile>> GetUsersByStatusAsync(string status)
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.Status == status)
                    .ToListAsync();

                var profiles = new List<UserProfile>();
                foreach (var user in users)
                {
                    profiles.Add(await MapToUserProfileAsync(user));
                }

                return profiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by status");
                return new List<UserProfile>();
            }
        }

        /// <summary>
        /// Get user with roles
        /// </summary>
        public async Task<UserProfile?> GetUserWithRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user != null ? await MapToUserProfileAsync(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with roles");
                return null;
            }
        }

        /// <summary>
        /// Check if user has permission
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user?.HasPermission(permission) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission");
                return false;
            }
        }

        /// <summary>
        /// Check if user is in role
        /// </summary>
        public async Task<bool> UserIsInRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user?.IsInRole(role) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user role");
                return false;
            }
        }

        /// <summary>
        /// Get user permissions
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user?.GetAllPermissions() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user?.GetAllRoles() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return new List<string>();
            }
        }

        /// <summary>
        /// Promote user to developer
        /// </summary>
        public async Task<UserManagementResponse> PromoteToDeveloperAsync(string userId)
        {
            return await AssignRoleAsync(userId, UserRoles.Developer, true);
        }

        /// <summary>
        /// Promote user to moderator
        /// </summary>
        public async Task<UserManagementResponse> PromoteToModeratorAsync(string userId)
        {
            return await AssignRoleAsync(userId, UserRoles.Moderator, true);
        }

        /// <summary>
        /// Demote user to basic user
        /// </summary>
        public async Task<UserManagementResponse> DemoteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserManagementResponse
                    {
                        Success = false,
                        Error = "User not found"
                    };
                }

                // Remove all roles except User
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles.Where(r => r != UserRoles.User))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                // Set primary role to User
                user.PrimaryRole = UserRoles.User;
                await _userManager.UpdateAsync(user);

                return new UserManagementResponse
                {
                    Success = true,
                    Message = "Successfully demoted user to basic user",
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error demoting user");
                return new UserManagementResponse
                {
                    Success = false,
                    Error = "Failed to demote user"
                };
            }
        }

        /// <summary>
        /// Validate role
        /// </summary>
        private bool IsValidRole(string role)
        {
            return role == UserRoles.SiteAdmin || 
                   role == UserRoles.Moderator || 
                   role == UserRoles.Developer || 
                   role == UserRoles.User;
        }

        /// <summary>
        /// Validate status
        /// </summary>
        private bool IsValidStatus(string status)
        {
            return status == "active" || status == "suspended" || status == "banned";
        }

        /// <summary>
        /// Map user to user profile
        /// </summary>
        private async Task<UserProfile> MapToUserProfileAsync(MarketplaceUser user)
        {
            var pluginCount = await _context.Plugins.CountAsync(p => p.AuthorId == user.Id);
            var installationCount = await _context.PluginInstallations.CountAsync(i => i.UserId == user.Id);
            var reviewCount = await _context.PluginReviews.CountAsync(r => r.UserId == user.Id);

            return new UserProfile
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                DisplayName = user.GitHubDisplayName,
                AvatarUrl = user.GitHubAvatarUrl,
                Bio = user.Bio,
                Location = user.Location,
                Website = user.Website,
                Company = user.Company,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LastActivityAt = user.LastActivityAt,
                GitHubId = user.GitHubId,
                GitHubUsername = user.GitHubUsername,
                GitHubProfileUrl = user.GitHubProfileUrl,
                PluginCount = pluginCount,
                InstallationCount = installationCount,
                ReviewCount = reviewCount,
                Status = user.Status,
                PrimaryRole = user.PrimaryRole,
                Roles = user.GetAllRoles(),
                Permissions = user.GetAllPermissions(),
                ReputationScore = user.ReputationScore,
                ContributionScore = user.ContributionScore
            };
        }
    }
}
