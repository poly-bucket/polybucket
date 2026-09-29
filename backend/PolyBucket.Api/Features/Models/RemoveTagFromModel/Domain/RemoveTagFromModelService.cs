using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.RemoveTagFromModel.Repository;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.RemoveTagFromModel.Domain;

public class RemoveTagFromModelService : IRemoveTagFromModelService
{
    private readonly IRemoveTagFromModelRepository _repository;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<RemoveTagFromModelService> _logger;

    public RemoveTagFromModelService(
        IRemoveTagFromModelRepository repository,
        IPermissionService permissionService,
        ILogger<RemoveTagFromModelService> logger)
    {
        _repository = repository;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task RemoveTagFromModelAsync(Guid modelId, Guid tagId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new ValidationException("Invalid authentication token");
        }

        var model = await _repository.GetModelAsync(modelId, cancellationToken);
        if (model == null)
        {
            throw new ModelNotFoundException($"Model with ID {modelId} not found");
        }

        if (model.IsDeleted)
        {
            throw new ValidationException("Cannot update a deleted model");
        }

        var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY);
        if (!hasAnyPermission && model.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this model");
        }

        var tag = model.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag == null)
        {
            throw new ModelNotFoundException($"Tag {tagId} is not linked to model {modelId}");
        }

        model.Tags.Remove(tag);
        model.UpdatedAt = DateTime.UtcNow;
        model.UpdatedById = userId;
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tag {TagId} unlinked from model {ModelId} by user {UserId}", tagId, modelId, userId);
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
