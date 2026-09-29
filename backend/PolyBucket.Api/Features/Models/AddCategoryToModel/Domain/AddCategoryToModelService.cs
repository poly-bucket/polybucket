using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Repository;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;

public class AddCategoryToModelService : IAddCategoryToModelService
{
    private readonly IAddCategoryToModelRepository _repository;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AddCategoryToModelService> _logger;

    public AddCategoryToModelService(
        IAddCategoryToModelRepository repository,
        IPermissionService permissionService,
        ILogger<AddCategoryToModelService> logger)
    {
        _repository = repository;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task AddCategoryToModelAsync(Guid modelId, Guid categoryId, ClaimsPrincipal user, CancellationToken cancellationToken)
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

        if (model.Categories.Any(c => c.Id == categoryId))
        {
            return;
        }

        var category = await _repository.GetActiveCategoryAsync(categoryId, cancellationToken);
        if (category == null)
        {
            throw new ModelNotFoundException($"Category with ID {categoryId} not found");
        }

        model.Categories.Add(category);
        model.UpdatedAt = DateTime.UtcNow;
        model.UpdatedById = userId;
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} linked to model {ModelId} by user {UserId}", categoryId, modelId, userId);
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
