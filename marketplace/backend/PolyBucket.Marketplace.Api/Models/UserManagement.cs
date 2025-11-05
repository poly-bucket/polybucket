using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Marketplace.Api.Models
{
    /// <summary>
    /// User roles in the marketplace
    /// </summary>
    public static class UserRoles
    {
        public const string SiteAdmin = "SiteAdmin";
        public const string Moderator = "Moderator";
        public const string Developer = "Developer";
        public const string User = "User";
    }

    /// <summary>
    /// Permissions available in the marketplace
    /// </summary>
    public static class Permissions
    {
        // User Management
        public const string ManageUsers = "ManageUsers";
        public const string ViewUserProfiles = "ViewUserProfiles";
        public const string AssignRoles = "AssignRoles";

        // Plugin Management
        public const string ApprovePlugins = "ApprovePlugins";
        public const string RejectPlugins = "RejectPlugins";
        public const string ManageAllPlugins = "ManageAllPlugins";
        public const string SubmitPlugins = "SubmitPlugins";
        public const string ManageOwnPlugins = "ManageOwnPlugins";

        // Content Management
        public const string ModerateContent = "ModerateContent";
        public const string DeleteReviews = "DeleteReviews";
        public const string ManageCategories = "ManageCategories";

        // Analytics
        public const string ViewAnalytics = "ViewAnalytics";
        public const string ViewUserAnalytics = "ViewUserAnalytics";

        // System Administration
        public const string SystemAdmin = "SystemAdmin";
        public const string ManageSettings = "ManageSettings";
    }

    /// <summary>
    /// Role-based permissions mapping
    /// </summary>
    public static class RolePermissions
    {
        public static readonly Dictionary<string, List<string>> RolePermissionMap = new()
        {
            [UserRoles.SiteAdmin] = new List<string>
            {
                Permissions.ManageUsers,
                Permissions.ViewUserProfiles,
                Permissions.AssignRoles,
                Permissions.ApprovePlugins,
                Permissions.RejectPlugins,
                Permissions.ManageAllPlugins,
                Permissions.SubmitPlugins,
                Permissions.ManageOwnPlugins,
                Permissions.ModerateContent,
                Permissions.DeleteReviews,
                Permissions.ManageCategories,
                Permissions.ViewAnalytics,
                Permissions.ViewUserAnalytics,
                Permissions.SystemAdmin,
                Permissions.ManageSettings
            },
            [UserRoles.Moderator] = new List<string>
            {
                Permissions.ViewUserProfiles,
                Permissions.ApprovePlugins,
                Permissions.RejectPlugins,
                Permissions.ModerateContent,
                Permissions.DeleteReviews,
                Permissions.ManageCategories,
                Permissions.ViewAnalytics
            },
            [UserRoles.Developer] = new List<string>
            {
                Permissions.SubmitPlugins,
                Permissions.ManageOwnPlugins,
                Permissions.ViewUserAnalytics
            },
            [UserRoles.User] = new List<string>
            {
                // Basic marketplace access - no special permissions needed
            }
        };
    }


    /// <summary>
    /// User management request models
    /// </summary>
    public class AssignRoleRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public bool IsPrimaryRole { get; set; } = false;
    }

    public class UpdateUserStatusRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Reason { get; set; }
    }

    public class UserManagementResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfile? User { get; set; }
        public string? Error { get; set; }
    }
}
