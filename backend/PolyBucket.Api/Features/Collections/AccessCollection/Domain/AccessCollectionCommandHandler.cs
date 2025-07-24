using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Collections.AccessCollection.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.AccessCollection.Domain
{
    public class AccessCollectionCommandHandler(
        ICollectionRepository repository,
        IHttpContextAccessor httpContextAccessor,
        IPasswordHasher passwordHasher) : IRequestHandler<AccessCollectionCommand, AccessCollectionResponse>
    {
        private readonly ICollectionRepository _repository = repository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public async Task<AccessCollectionResponse> Handle(AccessCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await _repository.GetCollectionByIdAsync(request.CollectionId);

            if (collection == null)
            {
                return new AccessCollectionResponse
                {
                    Success = false,
                    Message = "Collection not found"
                };
            }

            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = userId != null && collection.OwnerId == Guid.Parse(userId);

            // Public collections are always accessible
            if (collection.Visibility == CollectionVisibility.Public)
            {
                return new AccessCollectionResponse
                {
                    Success = true,
                    Collection = collection
                };
            }

            // Private collections - only owner can access
            if (collection.Visibility == CollectionVisibility.Private)
            {
                if (!isOwner)
                {
                    return new AccessCollectionResponse
                    {
                        Success = false,
                        Message = "This collection is private"
                    };
                }

                return new AccessCollectionResponse
                {
                    Success = true,
                    Collection = collection
                };
            }

            // Unlisted collections - owner can always access, others need password if set
            if (collection.Visibility == CollectionVisibility.Unlisted)
            {
                if (isOwner)
                {
                    return new AccessCollectionResponse
                    {
                        Success = true,
                        Collection = collection
                    };
                }

                // Check if password is required
                if (!string.IsNullOrEmpty(collection.PasswordHash))
                {
                    if (string.IsNullOrEmpty(request.Password))
                    {
                        return new AccessCollectionResponse
                        {
                            Success = false,
                            Message = "Password required to access this collection",
                            RequiresPassword = true
                        };
                    }

                    // Verify password
                    if (!_passwordHasher.VerifyPassword(request.Password, collection.PasswordHash))
                    {
                        return new AccessCollectionResponse
                        {
                            Success = false,
                            Message = "Incorrect password",
                            RequiresPassword = true
                        };
                    }
                }

                return new AccessCollectionResponse
                {
                    Success = true,
                    Collection = collection
                };
            }

            return new AccessCollectionResponse
            {
                Success = false,
                Message = "Access denied"
            };
        }
    }
} 