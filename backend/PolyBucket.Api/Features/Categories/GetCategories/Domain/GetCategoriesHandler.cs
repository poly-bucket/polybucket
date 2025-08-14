using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.GetCategories.Domain
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
    {
        private readonly IGetCategoriesService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetCategoriesHandler(IGetCategoriesService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User context not available");
            }

            return await _service.GetCategoriesAsync(user, request);
        }
    }
}
