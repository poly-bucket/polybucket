using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.GetUserCollections.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Domain
{
    public class GetUserCollectionsQueryHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetUserCollectionsQuery, IEnumerable<Collection>>
    {
        private readonly ICollectionRepository _repository = repository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<IEnumerable<Collection>> Handle(GetUserCollectionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return await _repository.GetCollectionsByUserIdAsync(Guid.Parse(userId));
        }
    }
} 