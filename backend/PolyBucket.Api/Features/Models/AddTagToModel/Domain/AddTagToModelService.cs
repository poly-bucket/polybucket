using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.AddTagToModel.Repository;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.AddTagToModel.Domain;

public class AddTagToModelService : IAddTagToModelService
{
    private readonly IAddTagToModelRepository _repository;
    private readonly IPermissionService _permissionService;
    private readonly IModelMetadataValidator _metadataValidator;
    private readonly IModelMetadataPolicy _metadataPolicy;
    private readonly ILogger<AddTagToModelService> _logger;

    public AddTagToModelService(
        IAddTagToModelRepository repository,
        IPermissionService permissionService,
        IModelMetadataValidator metadataValidator,
        IModelMetadataPolicy metadataPolicy,
        ILogger<AddTagToModelService> logger)
    {
        _repository = repository;
        _permissionService = permissionService;
        _metadataValidator = metadataValidator;
        _metadataPolicy = metadataPolicy;
        _logger = logger;
    }

    public async Task AddTagToModelAsync(Guid modelId, string? tagName, ClaimsPrincipal user, CancellationToken cancellationToken)
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

        var trimmed = tagName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ValidationException("Tag name cannot be empty");
        }

        if (model.Tags.Any(t => t.DeletedAt == null && string.Equals(t.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var activeTagCount = model.Tags.Count(t => t.DeletedAt == null);
        var metadataPolicy = await _metadataPolicy.GetSnapshotAsync(cancellationToken);
        var tagError = _metadataValidator.ValidateAddTag(metadataPolicy, activeTagCount, isNewLink: true);
        if (tagError != null)
        {
            throw new ValidationException(tagError);
        }

        var tag = await _repository.GetActiveTagByNameAsync(trimmed, cancellationToken);
        if (tag == null)
        {
            tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = trimmed,
                Color = string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                UpdatedById = userId
            };
            _repository.AddTag(tag);
        }
        else if (model.Tags.Any(t => t.Id == tag.Id))
        {
            return;
        }

        model.Tags.Add(tag);
        model.UpdatedAt = DateTime.UtcNow;
        model.UpdatedById = userId;
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tag {TagName} linked to model {ModelId} by user {UserId}", trimmed, modelId, userId);
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
