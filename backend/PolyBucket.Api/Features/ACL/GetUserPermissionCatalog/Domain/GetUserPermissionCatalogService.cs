using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;

namespace PolyBucket.Api.Features.ACL.GetUserPermissionCatalog.Domain;

internal static class UserPermissionCatalogFormatter
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
        if (parts.Length >= 3)
        {
            var action = parts[1];
            var resource = parts[2].Replace("_", " ");
            return $"Allows user to {action} {resource}";
        }

        return $"Permission: {permissionName}";
    }
}

public interface IGetUserPermissionCatalogService
{
    IReadOnlyList<PermissionCatalogItemDto> GetAllPermissions();
}

public class GetUserPermissionCatalogService : IGetUserPermissionCatalogService
{
    public IReadOnlyList<PermissionCatalogItemDto> GetAllPermissions()
    {
        var permissions = new List<PermissionCatalogItemDto>();
        var permissionType = typeof(PermissionConstants);
        var fields = permissionType.GetFields().Where(f => f.IsLiteral && !f.IsInitOnly);
        foreach (var field in fields)
        {
            var permissionName = field.GetValue(null)?.ToString();
            if (string.IsNullOrEmpty(permissionName))
            {
                continue;
            }

            permissions.Add(new PermissionCatalogItemDto
            {
                Name = permissionName,
                Category = UserPermissionCatalogFormatter.GetCategory(permissionName),
                Description = UserPermissionCatalogFormatter.GetDescription(permissionName)
            });
        }

        return permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList();
    }
}
