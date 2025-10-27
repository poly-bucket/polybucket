using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using PolyBucket.Api.Features.ACL.Services;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Files.Http
{
    public class PublicModelAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly PolyBucketDbContext _context;
        private readonly IPermissionService _permissionService;

        public PublicModelAuthorizationFilter(PolyBucketDbContext context, IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Extract modelId from route parameters
            if (!context.RouteData.Values.TryGetValue("modelId", out var modelIdValue) || 
                !Guid.TryParse(modelIdValue?.ToString(), out var modelId))
            {
                context.Result = new BadRequestResult();
                return;
            }

            // Get the model from database
            var model = await _context.Models
                .Include(m => m.Author)
                .FirstOrDefaultAsync(m => m.Id == modelId);

            if (model == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            // Public models are accessible to everyone
            if (model.Privacy == PrivacySettings.Public)
            {
                return; // Allow access
            }

            // For non-public models, require authentication
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Model owner can always access their own models
            if (model.AuthorId == currentUserId)
            {
                return; // Allow access
            }

            // Check if user has admin or moderator privileges
            var isAdmin = await _permissionService.IsAdminAsync(currentUserId);
            var userRole = await _permissionService.GetUserRoleAsync(currentUserId);
            var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
            
            if (isAdmin || isModerator)
            {
                return; // Allow access
            }

            // For Private models, only owner and admins/moderators can access
            if (model.Privacy == PrivacySettings.Private)
            {
                context.Result = new ForbidResult();
                return;
            }

            // For Unlisted models, anyone with the link can access
            if (model.Privacy == PrivacySettings.Unlisted)
            {
                return; // Allow access
            }

            // Default to forbidden
            context.Result = new ForbidResult();
        }
    }
} 