using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Domain
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
    {
        private readonly ICreateCategoryService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateCategoryHandler(ICreateCategoryService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User context not available");
            }

            return await _service.CreateCategoryAsync(request, user);
        }
    }
}
