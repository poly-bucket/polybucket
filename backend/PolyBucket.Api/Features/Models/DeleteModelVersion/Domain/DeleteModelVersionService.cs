using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Repository;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModelVersion.Domain
{
    public class DeleteModelVersionService : IDeleteModelVersionService
    {
        private readonly IDeleteModelVersionRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DeleteModelVersionService> _logger;

        public DeleteModelVersionService(IDeleteModelVersionRepository repository, IPermissionService permissionService, ILogger<DeleteModelVersionService> logger)
        {
            _repository = repository;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task DeleteModelVersionAsync(Guid modelId, Guid versionId, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new ValidationException("Invalid authentication token");
            }

            var modelVersion = await _repository.GetModelVersionAsync(modelId, versionId, cancellationToken);
            if (modelVersion == null)
            {
                throw new ModelVersionNotFoundException($"Model version with ID {versionId} not found for model {modelId}");
            }

            if (modelVersion.DeletedAt.HasValue)
            {
                throw new ModelVersionNotFoundException($"Model version with ID {versionId} not found for model {modelId}");
            }

            // Check ownership - user can only delete versions for their own models unless they have MODEL_DELETE_ANY permission
            var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_DELETE_ANY);
            if (!hasAnyPermission && modelVersion.Model.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this model version");
            }

            // Soft delete the model version
            modelVersion.DeletedAt = DateTime.UtcNow;
            modelVersion.DeletedById = userId;
            modelVersion.UpdatedAt = DateTime.UtcNow;
            modelVersion.UpdatedById = userId;

            // Soft delete associated files
            foreach (var file in modelVersion.Files)
            {
                file.DeletedAt = DateTime.UtcNow;
                file.DeletedById = userId;
                file.UpdatedAt = DateTime.UtcNow;
                file.UpdatedById = userId;
            }

            await _repository.DeleteModelVersionAsync(modelVersion, cancellationToken);

            _logger.LogInformation("Model version {ModelId}/{VersionId} soft deleted by user {UserId}", modelId, versionId, userId);
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException(string message) : base(message) { }
    }

    public class ModelVersionNotFoundException : Exception
    {
        public ModelVersionNotFoundException(string message) : base(message) { }
    }
} 