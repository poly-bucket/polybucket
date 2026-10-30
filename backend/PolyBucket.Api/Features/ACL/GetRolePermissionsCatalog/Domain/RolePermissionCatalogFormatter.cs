using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Domain;

internal static class RolePermissionCatalogFormatter
{
    public static string GetCategory(string permissionName) =>
        permissionName.Split('.')[0] switch
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

    public static string GetDescription(string permissionName)
    {
        var parts = permissionName.Split('.');
        if (parts.Length < 2)
        {
            return permissionName;
        }

        var action = parts[1].Replace('_', ' ');
        var resource = parts[0].Replace('_', ' ');
        return $"{action} {resource}";
    }
}
