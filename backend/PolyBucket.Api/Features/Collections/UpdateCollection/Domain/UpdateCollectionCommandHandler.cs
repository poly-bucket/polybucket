using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Collections.UpdateCollection.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Domain
{
    public class UpdateCollectionCommandHandler(
        ICollectionRepository collectionsRepository, 
        IHttpContextAccessor httpContextAccessor,
        IPasswordHasher passwordHasher) : IRequestHandler<UpdateCollectionCommand, Collection>
    {
        private readonly ICollectionRepository _collectionsRepository = collectionsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

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
            collection.Avatar = request.Avatar ?? collection.Avatar;

            // Handle password for unlisted collections
            if (request.Visibility == CollectionVisibility.Unlisted || collection.Visibility == CollectionVisibility.Unlisted)
            {
                if (!string.IsNullOrEmpty(request.Password))
                {
                    // Hash new password
                    var salt = _passwordHasher.GenerateSalt();
                    collection.PasswordHash = _passwordHasher.HashPassword(request.Password, salt);
                }
                else if (request.Visibility == CollectionVisibility.Unlisted && string.IsNullOrEmpty(request.Password))
                {
                    // If changing to unlisted without password, clear any existing password
                    collection.PasswordHash = null;
                }
            }
            else
            {
                // If not unlisted, clear password
                collection.PasswordHash = null;
            }

            return await _collectionsRepository.UpdateCollectionAsync(collection);
        }
    }
} 