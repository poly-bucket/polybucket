using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.CreateCollection.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Domain
{
    public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, Collection>
    {
        private readonly ICollectionRepository _collectionsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateCollectionCommandHandler(ICollectionRepository collectionsRepository, IHttpContextAccessor httpContextAccessor)
        {
            _collectionsRepository = collectionsRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Collection> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var collection = new Collection
            {
                Name = request.Name,
                Description = request.Description,
                Visibility = request.Visibility,
                OwnerId = Guid.Parse(userId)
            };

            return await _collectionsRepository.CreateCollectionAsync(collection);
        }
    }
} 