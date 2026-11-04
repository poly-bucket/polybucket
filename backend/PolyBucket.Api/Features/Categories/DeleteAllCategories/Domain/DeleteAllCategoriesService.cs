using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Categories.DeleteAllCategories.Repository;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Domain
{
    public class DeleteAllCategoriesService(
        IDeleteAllCategoriesRepository deleteAllCategoriesRepository,
        IPermissionService permissionService,
        ILogger<DeleteAllCategoriesService> logger) : IDeleteAllCategoriesService
    {
        private readonly IDeleteAllCategoriesRepository _deleteAllCategoriesRepository = deleteAllCategoriesRepository;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly ILogger<DeleteAllCategoriesService> _logger = logger;

        public async Task<DeleteAllCategoriesResponse> DeleteAllCategoriesAsync(
            DeleteAllCategoriesRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            if (string.IsNullOrEmpty(request.AdminPassword))
            {
                throw new UnauthorizedAccessException("Admin password is required");
            }

            if (!await _permissionService.HasPermissionAsync(userId, PermissionConstants.ADMIN_MANAGE_CATEGORIES))
            {
                throw new UnauthorizedAccessException("User lacks permission to manage categories");
            }

            _logger.LogWarning("Admin {AdminId} is attempting to delete ALL categories", userId);

            var deletedCount = await _deleteAllCategoriesRepository.DeleteAllCategoriesAndReturnCountAsync(
                userId,
                cancellationToken);

            if (deletedCount == 0)
            {
                return new DeleteAllCategoriesResponse
                {
                    Success = true,
                    Message = "No categories found to delete",
                    DeletedCount = 0,
                    DeletedAt = DateTime.UtcNow
                };
            }

            _logger.LogCritical(
                "Admin {AdminId} has deleted ALL {CategoryCount} categories from the system",
                userId,
                deletedCount);

            return new DeleteAllCategoriesResponse
            {
                Success = true,
                Message = $"Successfully deleted {deletedCount} categories",
                DeletedCount = deletedCount,
                DeletedAt = DateTime.UtcNow
            };
        }
    }
}
