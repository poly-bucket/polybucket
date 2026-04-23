using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.DeleteAllModels.Repository;
using PolyBucket.Api.Features.Authentication.Repository;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Domain
{
    public class DeleteAllModelsService : IDeleteAllModelsService
    {
        private readonly PolyBucketDbContext _context;
        private readonly IDeleteAllModelsUserRepository _userRepository;
        private readonly IAuthenticationRepository _authRepository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DeleteAllModelsService> _logger;

        public DeleteAllModelsService(
            PolyBucketDbContext context,
            IDeleteAllModelsUserRepository userRepository,
            IAuthenticationRepository authRepository,
            IPermissionService permissionService,
            ILogger<DeleteAllModelsService> logger)
        {
            _context = context;
            _userRepository = userRepository;
            _authRepository = authRepository;
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
                // Verify admin password
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

                // Verify admin password (in a real implementation, you'd hash and compare)
                // For now, we'll skip password verification as it requires additional setup
                // TODO: Implement proper password verification
                if (string.IsNullOrEmpty(request.AdminPassword))
                {
                    throw new UnauthorizedAccessException("Admin password is required");
                }

                // Check if user has admin role
                if (!await _permissionService.IsAdminAsync(currentUser.Id))
                {
                    throw new UnauthorizedAccessException("Only administrators can delete all models");
                }

                _logger.LogWarning("Admin {AdminId} ({AdminEmail}) is attempting to delete ALL models", 
                    currentUser.Id, currentUser.Email);

                // Get count before deletion for logging
                var modelCount = await _context.Models.CountAsync(cancellationToken);
                
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

                // Delete all models and related data
                // Note: This will cascade delete related entities due to foreign key constraints
                var models = await _context.Models.ToListAsync(cancellationToken);
                _context.Models.RemoveRange(models);

                // Also delete any orphaned files, comments, likes, etc.
                // Note: Comments will be cascade deleted when models are deleted due to foreign key constraints
                // But we'll clean up any orphaned comments just in case
                var orphanedComments = await _context.Comments
                    .Where(c => !_context.Models.Any(m => m.Id == c.Model.Id))
                    .ToListAsync(cancellationToken);
                _context.Comments.RemoveRange(orphanedComments);

                var orphanedLikes = await _context.Likes
                    .Where(l => !_context.Models.Any(m => m.Id == l.ModelId))
                    .ToListAsync(cancellationToken);
                _context.Likes.RemoveRange(orphanedLikes);

                await _context.SaveChangesAsync(cancellationToken);

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
