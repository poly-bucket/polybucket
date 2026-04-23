using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Models.DeleteAllModels.Repository;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Domain
{
    public class DeleteAllModelsService : IDeleteAllModelsService
    {
        private readonly IDeleteAllModelsRepository _deleteAllModelsRepository;
        private readonly IDeleteAllModelsUserRepository _userRepository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DeleteAllModelsService> _logger;

        public DeleteAllModelsService(
            IDeleteAllModelsRepository deleteAllModelsRepository,
            IDeleteAllModelsUserRepository userRepository,
            IPermissionService permissionService,
            ILogger<DeleteAllModelsService> logger)
        {
            _deleteAllModelsRepository = deleteAllModelsRepository;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<DeleteAllModelsResponse> DeleteAllModelsAsync(
            DeleteAllModelsRequest request, 
            ClaimsPrincipal user, 
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _userRepository.GetByIdAsNoTrackingAsync(Guid.Parse(userId), cancellationToken);
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }

                if (string.IsNullOrEmpty(request.AdminPassword))
                {
                    throw new UnauthorizedAccessException("Admin password is required");
                }

                if (!await _permissionService.IsAdminAsync(currentUser.Id))
                {
                    throw new UnauthorizedAccessException("Only administrators can delete all models");
                }

                _logger.LogWarning("Admin {AdminId} ({AdminEmail}) is attempting to delete ALL models", 
                    currentUser.Id, currentUser.Email);

                var modelCount = await _deleteAllModelsRepository.DeleteAllModelsAndReturnCountAsync(cancellationToken);
                
                if (modelCount == 0)
                {
                    return new DeleteAllModelsResponse
                    {
                        Success = true,
                        Message = "No models found to delete",
                        DeletedCount = 0,
                        DeletedAt = DateTime.UtcNow
                    };
                }

                _logger.LogCritical("Admin {AdminId} ({AdminEmail}) has deleted ALL {ModelCount} models from the system", 
                    currentUser.Id, currentUser.Email, modelCount);

                return new DeleteAllModelsResponse
                {
                    Success = true,
                    Message = $"Successfully deleted {modelCount} models",
                    DeletedCount = modelCount,
                    DeletedAt = DateTime.UtcNow
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting all models");
                throw new InvalidOperationException("An error occurred while deleting all models", ex);
            }
        }
    }
}
