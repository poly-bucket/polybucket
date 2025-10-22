using MediatR;
using PolyBucket.Api.Features.Collections.FavoriteCollection.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.FavoriteCollection.Domain
{
    public class FavoriteCollectionCommandHandler : IRequestHandler<FavoriteCollectionCommand, FavoriteCollectionResponse>
    {
        private readonly ICollectionRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FavoriteCollectionCommandHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FavoriteCollectionResponse> Handle(FavoriteCollectionCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return new FavoriteCollectionResponse
                {
                    Success = false,
                    Message = "User is not authenticated."
                };
            }

            var collection = await _repository.GetCollectionByIdAsync(request.CollectionId);
            if (collection == null)
            {
                return new FavoriteCollectionResponse
                {
                    Success = false,
                    Message = "Collection not found."
                };
            }

            // Users cannot favorite other users' collections
            if (collection.OwnerId != Guid.Parse(userId))
            {
                return new FavoriteCollectionResponse
                {
                    Success = false,
                    Message = "You can only favorite your own collections."
                };
            }

            // Update the favorite status
            collection.Favorite = request.IsFavorite;
            collection.UpdatedAt = DateTime.UtcNow;
            collection.UpdatedById = Guid.Parse(userId);

            await _repository.UpdateCollectionAsync(collection);

            return new FavoriteCollectionResponse
            {
                Success = true,
                Message = request.IsFavorite ? "Collection added to favorites." : "Collection removed from favorites.",
                IsFavorite = request.IsFavorite
            };
        }
    }
}
