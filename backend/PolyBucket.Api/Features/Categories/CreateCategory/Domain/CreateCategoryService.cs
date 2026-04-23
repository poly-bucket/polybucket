using PolyBucket.Api.Features.Categories.CreateCategory.Repository;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Domain
{
    public interface ICreateCategoryService
    {
        Task<CreateCategoryResponse> CreateCategoryAsync(CreateCategoryCommand command, ClaimsPrincipal user);
        Task<CreateCategoryResponse?> GetCategoryByIdAsync(Guid id);
    }

    public class CreateCategoryService : ICreateCategoryService
    {
        private readonly ICreateCategoryRepository _repository;
        private readonly IPermissionService _permissionService;

        public CreateCategoryService(ICreateCategoryRepository repository, IPermissionService permissionService)
        {
            _repository = repository;
            _permissionService = permissionService;
        }

        public async Task<CreateCategoryResponse> CreateCategoryAsync(CreateCategoryCommand command, ClaimsPrincipal user)
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

            var existingCategory = await _repository.GetCategoryByNameAsync(command.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Category with name '{command.Name}' already exists");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = command.Name.Trim(),
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = userId,
                UpdatedAt = DateTime.UtcNow
            };

            var createdCategory = await _repository.CreateCategoryAsync(category);

            return new CreateCategoryResponse
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
                CreatedAt = createdCategory.CreatedAt,
                CreatedById = createdCategory.CreatedById
            };
        }

        public async Task<CreateCategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _repository.GetCategoryByIdAsync(id);
            
            if (category == null)
            {
                return null;
            }

            return new CreateCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                CreatedById = category.CreatedById
            };
        }

        private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
