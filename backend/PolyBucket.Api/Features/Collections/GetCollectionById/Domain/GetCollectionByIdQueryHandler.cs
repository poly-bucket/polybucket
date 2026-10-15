using MediatR;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using PolyBucket.Api.Features.Collections.GetCollectionById.Repository;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Domain
{
    public class GetCollectionByIdQueryHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetCollectionByIdQuery, GetCollectionByIdResponseDto?>
    {
        private readonly ICollectionRepository _repository = repository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<GetCollectionByIdResponseDto?> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
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
                    return null;
                }
            }

            return GetCollectionByIdMapper.ToDto(collection);
        }
    }
} 