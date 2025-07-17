using MediatR;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.AddModelToCollection.Domain
{
    public class AddModelToCollectionCommandHandler : IRequestHandler<AddModelToCollectionCommand>
    {
        private readonly ICollectionRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddModelToCollectionCommandHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Handle(AddModelToCollectionCommand request, CancellationToken cancellationToken)
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

            // Check if model exists
            var modelExists = await _repository.ModelExistsAsync(request.ModelId);
            if (!modelExists)
            {
                throw new KeyNotFoundException("Model not found.");
            }

            // Check if model is already in collection
            var isModelInCollection = await _repository.IsModelInCollectionAsync(request.CollectionId, request.ModelId);
            if (isModelInCollection)
            {
                throw new InvalidOperationException("Model is already in this collection.");
            }

            await _repository.AddModelToCollectionAsync(request.CollectionId, request.ModelId);
        }
    }
} 