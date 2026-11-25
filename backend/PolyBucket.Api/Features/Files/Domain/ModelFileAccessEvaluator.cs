using System.Security.Claims;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.Files.Domain;

public interface IModelFileAccessEvaluator
{
    Task<bool> CanUserAccessModelAsync(Model model, ClaimsPrincipal user, CancellationToken cancellationToken = default);

    Task<ModelDownloadAccessResult> CanDownloadModelAsync(
        Model model,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}

public class ModelFileAccessEvaluator(
    IPermissionService permissionService,
    ISiteModelDownloadPolicy downloadPolicy) : IModelFileAccessEvaluator
{
    public async Task<bool> CanUserAccessModelAsync(Model model, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var result = await CanDownloadModelAsync(model, user, cancellationToken);
        return result == ModelDownloadAccessResult.Allowed;
    }

    public async Task<ModelDownloadAccessResult> CanDownloadModelAsync(
        Model model,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (!await downloadPolicy.AreModelDownloadsEnabledAsync(cancellationToken))
        {
            return ModelDownloadAccessResult.Disabled;
        }

        var isAuthenticated = user.Identity?.IsAuthenticated == true;

        if (!isAuthenticated)
        {
            if (!await downloadPolicy.IsAnonymousModelDownloadAllowedAsync(cancellationToken))
            {
                return ModelDownloadAccessResult.SignInRequired;
            }

            return model.Privacy switch
            {
                PrivacySettings.Public => ModelDownloadAccessResult.Allowed,
                PrivacySettings.Unlisted => ModelDownloadAccessResult.Allowed,
                _ => ModelDownloadAccessResult.Forbidden
            };
        }

        if (model.Privacy == PrivacySettings.Public)
        {
            return ModelDownloadAccessResult.Allowed;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return ModelDownloadAccessResult.Forbidden;
        }

        if (model.AuthorId == currentUserId)
        {
            return ModelDownloadAccessResult.Allowed;
        }

        var isAdmin = await permissionService.IsAdminAsync(currentUserId);
        var userRole = await permissionService.GetUserRoleAsync(currentUserId);
        var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
        if (isAdmin || isModerator)
        {
            return ModelDownloadAccessResult.Allowed;
        }

        if (model.Privacy == PrivacySettings.Private)
        {
            return ModelDownloadAccessResult.Forbidden;
        }

        if (model.Privacy == PrivacySettings.Unlisted)
        {
            return ModelDownloadAccessResult.Allowed;
        }

        return ModelDownloadAccessResult.Forbidden;
    }
}