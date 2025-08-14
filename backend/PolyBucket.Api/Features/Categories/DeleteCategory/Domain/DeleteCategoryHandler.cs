using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Domain
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
    {
        private readonly IDeleteCategoryService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteCategoryHandler(IDeleteCategoryService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User context not available");
            }

            return await _service.DeleteCategoryAsync(request, user);
        }
    }
}
