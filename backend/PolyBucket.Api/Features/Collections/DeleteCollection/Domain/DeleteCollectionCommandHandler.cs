using MediatR;
using PolyBucket.Api.Features.Collections.DeleteCollection.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteCollection.Domain
{
    public class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand>
    {
        private readonly ICollectionRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteCollectionCommandHandler(ICollectionRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var collection = await _repository.GetCollectionByIdAsync(request.Id);
            if (collection != null)
            {
                if (collection.OwnerId != Guid.Parse(userId))
                {
                    throw new UnauthorizedAccessException("User is not authorized to delete this collection.");
                }

                await _repository.DeleteCollectionAsync(collection);
            }
        }
    }
} 