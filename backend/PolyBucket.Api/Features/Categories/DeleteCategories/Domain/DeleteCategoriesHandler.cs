using MediatR;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Categories.DeleteCategory.Domain;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategories.Domain
{
    public class DeleteCategoriesHandler : IRequestHandler<DeleteCategoriesCommand, DeleteCategoriesResponse>
    {
        private readonly IDeleteCategoryService _deleteCategoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteCategoriesHandler(
            IDeleteCategoryService deleteCategoryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _deleteCategoryService = deleteCategoryService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DeleteCategoriesResponse> Handle(
            DeleteCategoriesCommand request,
            CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User context not available");
            }

            var distinctIds = request.Ids.Distinct().ToList();
            var results = new List<DeleteCategoryItemResult>();

            foreach (var id in distinctIds)
            {
                try
                {
                    var deleteResult = await _deleteCategoryService.DeleteCategoryAsync(
                        new DeleteCategoryCommand { Id = id },
                        user);

                    results.Add(new DeleteCategoryItemResult
                    {
                        Id = id,
                        Name = deleteResult.Name,
                        Success = true,
                        Message = deleteResult.Message
                    });
                }
                catch (InvalidOperationException ex)
                {
                    results.Add(new DeleteCategoryItemResult
                    {
                        Id = id,
                        Success = false,
                        Message = ex.Message
                    });
                }
            }

            return new DeleteCategoriesResponse
            {
                DeletedCount = results.Count(r => r.Success),
                FailedCount = results.Count(r => !r.Success),
                Results = results
            };
        }
    }
}
