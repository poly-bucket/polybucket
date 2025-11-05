using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Marketplace.Api.Models
{
    /// <summary>
    /// User model for marketplace authentication
    /// </summary>
    public class MarketplaceUser : IdentityUser
    {
        /// <summary>
        /// GitHub user ID
        /// </summary>
        public long? GitHubId { get; set; }

        /// <summary>
        /// GitHub username
        /// </summary>
        public string? GitHubUsername { get; set; }

        /// <summary>
        /// GitHub display name
        /// </summary>
        public string? GitHubDisplayName { get; set; }

        /// <summary>
        /// GitHub avatar URL
        /// </summary>
        public string? GitHubAvatarUrl { get; set; }

        /// <summary>
        /// GitHub profile URL
        /// </summary>
        public string? GitHubProfileUrl { get; set; }

        /// <summary>
        /// GitHub access token (encrypted)
        /// </summary>
        public string? GitHubAccessToken { get; set; }

        /// <summary>
        /// GitHub refresh token (encrypted)
        /// </summary>
        public string? GitHubRefreshToken { get; set; }

        /// <summary>
        /// Token expiration time
        /// </summary>
        public DateTime? TokenExpiresAt { get; set; }

        /// <summary>
        /// User's bio/description
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// User's location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// User's website
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// User's company
        /// </summary>
        public string? Company { get; set; }

        /// <summary>
        /// Whether user is verified
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// User status (active, suspended, banned)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// User creation date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last login date
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Last activity date
        /// </summary>
        public DateTime? LastActivityAt { get; set; }

        /// <summary>
        /// User's primary role
        /// </summary>
        public string PrimaryRole { get; set; } = UserRoles.User;

        /// <summary>
        /// Additional roles (comma-separated)
        /// </summary>
        public string? AdditionalRoles { get; set; }

        /// <summary>
        /// Custom permissions (comma-separated)
        /// </summary>
        public string? CustomPermissions { get; set; }

        /// <summary>
        /// User's reputation score
        /// </summary>
        public int ReputationScore { get; set; } = 0;

        /// <summary>
        /// User's contribution score
        /// </summary>
        public int ContributionScore { get; set; } = 0;

        /// <summary>
        /// Navigation property for user's plugins
        /// </summary>
        public virtual ICollection<Plugin> Plugins { get; set; } = new List<Plugin>();

        /// <summary>
        /// Navigation property for user's installations
        /// </summary>
        public virtual ICollection<PluginInstallation> Installations { get; set; } = new List<PluginInstallation>();

        /// <summary>
        /// Navigation property for user's reviews
        /// </summary>
        public virtual ICollection<PluginReview> Reviews { get; set; } = new List<PluginReview>();

        /// <summary>
        /// Navigation property for user's submissions
        /// </summary>
        public virtual ICollection<PluginSubmission> Submissions { get; set; } = new List<PluginSubmission>();

        /// <summary>
        /// Get all roles for this user
        /// </summary>
        public List<string> GetAllRoles()
        {
            var roles = new List<string> { PrimaryRole };
            
            if (!string.IsNullOrEmpty(AdditionalRoles))
            {
                roles.AddRange(AdditionalRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim()));
            }
            
            return roles.Distinct().ToList();
        }

        /// <summary>
        /// Get all permissions for this user
        /// </summary>
        public List<string> GetAllPermissions()
        {
            var permissions = new HashSet<string>();
            
            // Add permissions from roles
            foreach (var role in GetAllRoles())
            {
                if (RolePermissions.RolePermissionMap.TryGetValue(role, out var rolePermissions))
                {
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission);
                    }
                }
            }
            
            // Add custom permissions
            if (!string.IsNullOrEmpty(CustomPermissions))
            {
                foreach (var permission in CustomPermissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()))
                {
                    permissions.Add(permission);
                }
            }
            
            return permissions.ToList();
        }

        /// <summary>
        /// Check if user has a specific permission
        /// </summary>
        public bool HasPermission(string permission)
        {
            return GetAllPermissions().Contains(permission);
        }

        /// <summary>
        /// Check if user has any of the specified permissions
        /// </summary>
        public bool HasAnyPermission(params string[] permissions)
        {
            var userPermissions = GetAllPermissions();
            return permissions.Any(p => userPermissions.Contains(p));
        }

        /// <summary>
        /// Check if user has all of the specified permissions
        /// </summary>
        public bool HasAllPermissions(params string[] permissions)
        {
            var userPermissions = GetAllPermissions();
            return permissions.All(p => userPermissions.Contains(p));
        }

        /// <summary>
        /// Check if user is in a specific role
        /// </summary>
        public bool IsInRole(string role)
        {
            return GetAllRoles().Contains(role);
        }

        /// <summary>
        /// Check if user is admin or moderator
        /// </summary>
        public bool IsAdminOrModerator()
        {
            return IsInRole(UserRoles.SiteAdmin) || IsInRole(UserRoles.Moderator);
        }

        /// <summary>
        /// Check if user is developer or higher
        /// </summary>
        public bool IsDeveloperOrHigher()
        {
            return IsInRole(UserRoles.SiteAdmin) || IsInRole(UserRoles.Moderator) || IsInRole(UserRoles.Developer);
        }
    }

    /// <summary>
    /// GitHub OAuth callback model
    /// </summary>
    public class GitHubOAuthCallback
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        public string? State { get; set; }
    }

    /// <summary>
    /// Authentication response model
    /// </summary>
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserProfile? User { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// User profile model for API responses
    /// </summary>
    public class UserProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? Company { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public long? GitHubId { get; set; }
        public string? GitHubUsername { get; set; }
        public string? GitHubProfileUrl { get; set; }
        public int PluginCount { get; set; }
        public int InstallationCount { get; set; }
        public int ReviewCount { get; set; }
        public string Status { get; set; } = "active";
        public string PrimaryRole { get; set; } = UserRoles.User;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public int ReputationScore { get; set; }
        public int ContributionScore { get; set; }
    }

    /// <summary>
    /// GitHub user info from API
    /// </summary>
    public class GitHubUserInfo
    {
        public long Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar_Url { get; set; } = string.Empty;
        public string Html_Url { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Blog { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public bool Site_Admin { get; set; }
        public int Public_Repos { get; set; }
        public int Public_Gists { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }

    /// <summary>
    /// GitHub access token response
    /// </summary>
    public class GitHubTokenResponse
    {
        public string Access_Token { get; set; } = string.Empty;
        public string Token_Type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string? Refresh_Token { get; set; }
        public int? Expires_In { get; set; }
    }
}
