using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.DeleteModel.Repository;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModel.Domain
{
    public class DeleteModelService : IDeleteModelService
    {
        private readonly IDeleteModelRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DeleteModelService> _logger;

        public DeleteModelService(IDeleteModelRepository repository, IPermissionService permissionService, ILogger<DeleteModelService> logger)
        {
            _repository = repository;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task DeleteModelAsync(Guid modelId, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new ValidationException("Invalid authentication token");
            }

            var model = await _repository.GetModelByIdAsync(modelId, cancellationToken);
            if (model == null)
            {
                throw new ModelNotFoundException($"Model with ID {modelId} not found");
            }

            if (model.IsDeleted)
            {
                throw new ModelNotFoundException($"Model with ID {modelId} not found");
            }

            // Check ownership - user can only delete their own models unless they have MODEL_DELETE_ANY permission
            var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_DELETE_ANY);
            if (!hasAnyPermission && model.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this model");
            }

            // Soft delete the model
            model.DeletedAt = DateTime.UtcNow;
            model.DeletedById = userId;
            model.UpdatedAt = DateTime.UtcNow;
            model.UpdatedById = userId;

            // Soft delete associated files
            foreach (var file in model.Files)
            {
                file.DeletedAt = DateTime.UtcNow;
                file.DeletedById = userId;
                file.UpdatedAt = DateTime.UtcNow;
                file.UpdatedById = userId;
            }

            await _repository.DeleteModelAsync(model, cancellationToken);

            _logger.LogInformation("Model {ModelId} soft deleted by user {UserId}", modelId, userId);
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
} 