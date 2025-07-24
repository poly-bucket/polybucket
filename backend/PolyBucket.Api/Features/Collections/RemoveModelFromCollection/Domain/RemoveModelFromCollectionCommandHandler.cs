using MediatR;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Domain
{
    public class RemoveModelFromCollectionCommandHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<RemoveModelFromCollectionCommand>
    {
        private readonly ICollectionRepository _repository = repository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task Handle(RemoveModelFromCollectionCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var collection = await _repository.GetCollectionByIdAsync(request.CollectionId);
            if (collection == null)
            {
                throw new KeyNotFoundException("Collection not found.");
            }

            if (collection.OwnerId != Guid.Parse(userId))
            {
                throw new UnauthorizedAccessException("User is not authorized to modify this collection.");
            }

            // Check if model is in collection
            var isModelInCollection = await _repository.IsModelInCollectionAsync(request.CollectionId, request.ModelId);
            if (!isModelInCollection)
            {
                throw new InvalidOperationException("Model is not in this collection.");
            }

            await _repository.RemoveModelFromCollectionAsync(request.CollectionId, request.ModelId);
        }
    }
} 