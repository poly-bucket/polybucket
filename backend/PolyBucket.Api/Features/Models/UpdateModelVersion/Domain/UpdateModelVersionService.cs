using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Http;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Repository;
using PolyBucket.Api.Common.Models;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Domain
{
    public class UpdateModelVersionService : IUpdateModelVersionService
    {
        private readonly IUpdateModelVersionRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<UpdateModelVersionService> _logger;

        public UpdateModelVersionService(IUpdateModelVersionRepository repository, IPermissionService permissionService, ILogger<UpdateModelVersionService> logger)
        {
            _repository = repository;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<UpdateModelVersionResponse> UpdateModelVersionAsync(Guid modelId, Guid versionId, UpdateModelVersionRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
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
                throw new ValidationException("Cannot update a deleted model version");
            }

            // Check ownership - user can only edit versions for their own models unless they have MODEL_EDIT_ANY permission
            var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY);
            if (!hasAnyPermission && modelVersion.Model.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this model version");
            }

            // Update model version properties
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                modelVersion.Name = request.Name;
            }

            if (request.Notes != null)
            {
                modelVersion.Notes = request.Notes;
            }

            // Update audit fields
            modelVersion.UpdatedAt = DateTime.UtcNow;
            modelVersion.UpdatedById = userId;

            await _repository.UpdateModelVersionAsync(modelVersion, cancellationToken);

            _logger.LogInformation("Model version {ModelId}/{VersionId} updated by user {UserId}", modelId, versionId, userId);

            return new UpdateModelVersionResponse { ModelVersion = modelVersion };
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