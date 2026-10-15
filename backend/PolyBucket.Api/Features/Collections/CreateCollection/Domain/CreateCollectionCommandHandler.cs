using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Collections.CreateCollection.Http;
using PolyBucket.Api.Features.Collections.CreateCollection.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Domain
{
    public class CreateCollectionCommandHandler(
        ICollectionRepository collectionsRepository, 
        IHttpContextAccessor httpContextAccessor,
        IPasswordHasher passwordHasher) : IRequestHandler<CreateCollectionCommand, CreateCollectionResponseDto>
    {
        private readonly ICollectionRepository _collectionsRepository = collectionsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public async Task<CreateCollectionResponseDto> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
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
                Avatar = request.Avatar,
                OwnerId = Guid.Parse(userId),
                CreatedAt = DateTime.UtcNow,
                CreatedById = Guid.Parse(userId),
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = Guid.Parse(userId)
            };

            // Hash password if provided for unlisted collections
            if (request.Visibility == CollectionVisibility.Unlisted && !string.IsNullOrEmpty(request.Password))
            {
                var salt = _passwordHasher.GenerateSalt();
                collection.PasswordHash = _passwordHasher.HashPassword(request.Password, salt);
            }

            var created = await _collectionsRepository.CreateCollectionAsync(collection);
            return CreateCollectionMapper.ToResponseDto(created);
        }
    }
} 