using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.UpdateCollection.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Domain
{
    public class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, Collection>
    {
        private readonly ICollectionRepository _collectionsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateCollectionCommandHandler(ICollectionRepository collectionsRepository, IHttpContextAccessor httpContextAccessor)
        {
            _collectionsRepository = collectionsRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Collection> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var collection = await _collectionsRepository.GetCollectionByIdAsync(request.Id);
            if (collection == null)
            {
                throw new KeyNotFoundException("Collection not found.");
            }

            if (collection.OwnerId != Guid.Parse(userId))
            {
                throw new UnauthorizedAccessException("User is not authorized to update this collection.");
            }

            collection.Name = request.Name ?? collection.Name;
            collection.Description = request.Description ?? collection.Description;
            collection.Visibility = request.Visibility ?? collection.Visibility;

            return await _collectionsRepository.UpdateCollectionAsync(collection);
        }
    }
} 