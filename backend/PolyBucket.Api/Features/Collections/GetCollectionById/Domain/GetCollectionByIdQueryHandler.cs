using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.GetCollectionById.Repository;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Domain
{
    public class GetCollectionByIdQueryHandler : IRequestHandler<GetCollectionByIdQuery, Collection?>
    {
        private readonly ICollectionRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetCollectionByIdQueryHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Collection?> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
        {
            var collection = await _repository.GetCollectionByIdAsync(request.Id);

            if (collection == null)
            {
                return null;
            }

            if (collection.Visibility == CollectionVisibility.Private || collection.Visibility == CollectionVisibility.Unlisted)
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null || collection.OwnerId != Guid.Parse(userId))
                {
                    // For now, only the owner can view private/unlisted. This will be expanded with CollectionMember logic.
                    return null;
                }
            }

            return collection;
        }
    }
} 