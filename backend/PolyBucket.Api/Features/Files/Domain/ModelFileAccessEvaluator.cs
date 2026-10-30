using System;
        using System.Security.Claims;
        using System.Threading;
        using System.Threading.Tasks;
        using PolyBucket.Api.Common.Enums;
        using PolyBucket.Api.Features.ACL.Services;
        using PolyBucket.Database.Entities.Models;
        using PolyBucket.Database.Entities.Models.Enums;

        namespace PolyBucket.Api.Features.Files.Domain;

        public interface IModelFileAccessEvaluator
        {
            Task<bool> CanUserAccessModelAsync(Model model, ClaimsPrincipal user, CancellationToken cancellationToken = default);
        }

        public class ModelFileAccessEvaluator(IPermissionService permissionService) : IModelFileAccessEvaluator
        {
            public async Task<bool> CanUserAccessModelAsync(Model model, ClaimsPrincipal user, CancellationToken cancellationToken = default)
            {
                if (model.Privacy == PrivacySettings.Public)
                {
                    return true;
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                {
                    return false;
                }

                if (model.AuthorId == currentUserId)
                {
                    return true;
                }

                var isAdmin = await permissionService.IsAdminAsync(currentUserId);
                var userRole = await permissionService.GetUserRoleAsync(currentUserId);
                var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
                if (isAdmin || isModerator)
                {
                    return true;
                }

                if (model.Privacy == PrivacySettings.Private)
                {
                    return false;
                }

                if (model.Privacy == PrivacySettings.Unlisted)
                {
                    return true;
                }

                return false;
            }
        }
