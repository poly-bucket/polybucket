using PolyBucket.Api.Features.Categories.UpdateCategory.Repository;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Domain
{
    public interface IUpdateCategoryService
    {
        Task<UpdateCategoryResponse> UpdateCategoryAsync(UpdateCategoryCommand command, ClaimsPrincipal user);
    }

    public class UpdateCategoryService : IUpdateCategoryService
    {
        private readonly IUpdateCategoryRepository _repository;
        private readonly IPermissionService _permissionService;

        public UpdateCategoryService(IUpdateCategoryRepository repository, IPermissionService permissionService)
        {
            _repository = repository;
            _permissionService = permissionService;
        }

        public async Task<UpdateCategoryResponse> UpdateCategoryAsync(UpdateCategoryCommand command, ClaimsPrincipal user)
        {
            var userId = GetUserIdFromClaims(user);
            
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            if (!await _permissionService.HasPermissionAsync(userId, PermissionConstants.ADMIN_MANAGE_CATEGORIES))
            {
                throw new UnauthorizedAccessException("User lacks permission to manage categories");
            }

            var existingCategory = await _repository.GetCategoryByIdAsync(command.Id);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Category with ID '{command.Id}' not found");
            }

            var categoryWithSameName = await _repository.GetCategoryByNameAsync(command.Name);
            if (categoryWithSameName != null && categoryWithSameName.Id != command.Id)
            {
                throw new InvalidOperationException($"Category with name '{command.Name}' already exists");
            }

            var updatedCategory = await _repository.UpdateCategoryAsync(command.Id, command.Name, userId);

            return new UpdateCategoryResponse
            {
                Id = updatedCategory.Id,
                Name = updatedCategory.Name,
                UpdatedAt = updatedCategory.UpdatedAt ?? DateTime.UtcNow,
                UpdatedById = updatedCategory.UpdatedById
            };
        }

        private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
