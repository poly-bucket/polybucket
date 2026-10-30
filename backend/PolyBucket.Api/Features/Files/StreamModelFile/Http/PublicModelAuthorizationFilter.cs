using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Persistence;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Files.StreamModelFile.Http;

public class PublicModelAuthorizationFilter(PolyBucketDbContext dbContext, IPermissionService permissionService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.RouteData.Values.TryGetValue("modelId", out var modelIdValue) ||
            !Guid.TryParse(modelIdValue?.ToString(), out var modelId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var model = await dbContext.Models
            .Include(m => m.Author)
            .FirstOrDefaultAsync(m => m.Id == modelId);

        if (model == null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        if (model.Privacy == PrivacySettings.Public)
        {
            return;
        }

        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (model.AuthorId == currentUserId)
        {
            return;
        }

        var isAdmin = await permissionService.IsAdminAsync(currentUserId);
        var userRole = await permissionService.GetUserRoleAsync(currentUserId);
        var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
        if (isAdmin || isModerator)
        {
            return;
        }

        if (model.Privacy == PrivacySettings.Private)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (model.Privacy == PrivacySettings.Unlisted)
        {
            return;
        }

        context.Result = new ForbidResult();
    }
}
