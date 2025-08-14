using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Domain
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
    {
        private readonly IUpdateCategoryService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateCategoryHandler(IUpdateCategoryService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User context not available");
            }

            return await _service.UpdateCategoryAsync(request, user);
        }
    }
}
