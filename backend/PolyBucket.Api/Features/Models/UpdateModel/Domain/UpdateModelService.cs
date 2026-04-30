using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.UpdateModel.Http;
using PolyBucket.Api.Features.Models.UpdateModel.Repository;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModel.Domain
{
    public class UpdateModelService : IUpdateModelService
    {
        private readonly IUpdateModelRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<UpdateModelService> _logger;

        public UpdateModelService(IUpdateModelRepository repository, IPermissionService permissionService, ILogger<UpdateModelService> logger)
        {
            _repository = repository;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<UpdateModelResponse> UpdateModelAsync(Guid modelId, UpdateModelRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
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
                throw new ValidationException("Cannot update a deleted model");
            }

            // Check ownership - user can only edit their own models unless they have MODEL_EDIT_ANY permission
            var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY);
            if (!hasAnyPermission && model.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this model");
            }

            if (request.Name != null && string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("Model name cannot be empty");
            }

            // Update model properties
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                model.Name = request.Name;
            }

            if (request.Description != null)
            {
                model.Description = request.Description;
            }

            if (request.License.HasValue)
            {
                model.License = request.License.Value;
            }

            if (request.Privacy.HasValue)
            {
                model.Privacy = request.Privacy.Value;
                model.IsPublic = request.Privacy.Value == PrivacySettings.Public;
            }

            if (request.AIGenerated.HasValue)
            {
                model.AIGenerated = request.AIGenerated.Value;
            }

            if (request.WIP.HasValue)
            {
                model.WIP = request.WIP.Value;
            }

            if (request.NSFW.HasValue)
            {
                model.NSFW = request.NSFW.Value;
            }

            if (request.IsRemix.HasValue)
            {
                model.IsRemix = request.IsRemix.Value;
            }

            if (request.RemixUrl != null)
            {
                model.RemixUrl = request.RemixUrl;
            }

            if (request.IsFeatured.HasValue)
            {
                model.IsFeatured = request.IsFeatured.Value;
            }

            // Update audit fields
            model.UpdatedAt = DateTime.UtcNow;
            model.UpdatedById = userId;

            await _repository.UpdateModelAsync(model, cancellationToken);

            _logger.LogInformation("Model {ModelId} updated by user {UserId}", modelId, userId);

            return new UpdateModelResponse { Model = model };
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