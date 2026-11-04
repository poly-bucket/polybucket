using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Collections.DeleteAllCollections.Repository;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Domain
{
    public class DeleteAllCollectionsService(
        IDeleteAllCollectionsRepository deleteAllCollectionsRepository,
        IPermissionService permissionService,
        ILogger<DeleteAllCollectionsService> logger) : IDeleteAllCollectionsService
    {
        private readonly IDeleteAllCollectionsRepository _deleteAllCollectionsRepository = deleteAllCollectionsRepository;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly ILogger<DeleteAllCollectionsService> _logger = logger;

        public async Task<DeleteAllCollectionsResponse> DeleteAllCollectionsAsync(
            DeleteAllCollectionsRequest request,
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

            if (!await _permissionService.HasPermissionAsync(userId, PermissionConstants.COLLECTION_DELETE_ANY))
            {
                throw new UnauthorizedAccessException("User lacks permission to delete any collection");
            }

            _logger.LogWarning("Admin {AdminId} is attempting to delete ALL collections", userId);

            var deletedCount = await _deleteAllCollectionsRepository.DeleteAllCollectionsAndReturnCountAsync(cancellationToken);

            if (deletedCount == 0)
            {
                return new DeleteAllCollectionsResponse
                {
                    Success = true,
                    Message = "No collections found to delete",
                    DeletedCount = 0,
                    DeletedAt = DateTime.UtcNow
                };
            }

            _logger.LogCritical(
                "Admin {AdminId} has deleted ALL {CollectionCount} collections from the system",
                userId,
                deletedCount);

            return new DeleteAllCollectionsResponse
            {
                Success = true,
                Message = $"Successfully deleted {deletedCount} collections",
                DeletedCount = deletedCount,
                DeletedAt = DateTime.UtcNow
            };
        }
    }
}
