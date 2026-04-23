using PolyBucket.Api.Features.Categories.DeleteCategory.Repository;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Domain
{
    public interface IDeleteCategoryService
    {
        Task<DeleteCategoryResponse> DeleteCategoryAsync(DeleteCategoryCommand command, ClaimsPrincipal user);
    }

    public class DeleteCategoryService : IDeleteCategoryService
    {
        private readonly IDeleteCategoryRepository _repository;
        private readonly IPermissionService _permissionService;

        public DeleteCategoryService(IDeleteCategoryRepository repository, IPermissionService permissionService)
        {
            _repository = repository;
            _permissionService = permissionService;
        }

        public async Task<DeleteCategoryResponse> DeleteCategoryAsync(DeleteCategoryCommand command, ClaimsPrincipal user)
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

            var category = await _repository.GetCategoryByIdAsync(command.Id);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID '{command.Id}' not found");
            }

            var isInUse = await _repository.IsCategoryInUseAsync(command.Id);
            if (isInUse)
            {
                throw new InvalidOperationException($"Category '{category.Name}' cannot be deleted because it is in use by models");
            }

            var deletedCategory = await _repository.DeleteCategoryAsync(command.Id, userId);

            return new DeleteCategoryResponse
            {
                Id = deletedCategory.Id,
                Name = deletedCategory.Name,
                Success = true,
                Message = $"Category '{deletedCategory.Name}' deleted successfully",
                DeletedAt = deletedCategory.DeletedAt ?? DateTime.UtcNow,
                DeletedById = deletedCategory.DeletedById ?? userId
            };
        }

        private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
