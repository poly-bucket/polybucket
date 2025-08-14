using PolyBucket.Api.Features.Categories.GetCategories.Repository;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace PolyBucket.Api.Features.Categories.GetCategories.Domain
{
    public interface IGetCategoriesService
    {
        Task<GetCategoriesResponse> GetCategoriesAsync(ClaimsPrincipal user, GetCategoriesQuery query);
    }

    public class GetCategoriesService : IGetCategoriesService
    {
        private readonly IGetCategoriesRepository _repository;
        private readonly IPermissionService _permissionService;

        public GetCategoriesService(IGetCategoriesRepository repository, IPermissionService permissionService)
        {
            _repository = repository;
            _permissionService = permissionService;
        }

        public async Task<GetCategoriesResponse> GetCategoriesAsync(ClaimsPrincipal user, GetCategoriesQuery query)
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

            return await _repository.GetCategoriesAsync(query);
        }

        private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
