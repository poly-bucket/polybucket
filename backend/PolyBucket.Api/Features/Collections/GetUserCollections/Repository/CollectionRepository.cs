using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Collections.GetUserCollections.Domain;
using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Repository
{
    public class CollectionRepository(PolyBucketDbContext context) : ICollectionRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<GetUserCollectionsResponseDto> GetUserCollectionsAsync(GetUserCollectionsQuery query, CancellationToken cancellationToken = default)
        {
            var targetUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == query.TargetUserId, cancellationToken);

            if (targetUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var canViewPrivate = query.IsRequestingUserAdmin
                || (query.RequestingUserId.HasValue && query.RequestingUserId.Value == targetUser.Id);

            if (!targetUser.IsProfilePublic && !canViewPrivate)
            {
                throw new UnauthorizedAccessException("User profile is private");
            }

            var collectionsQuery = _context.Collections
                .AsNoTracking()
                .Where(c => c.OwnerId == targetUser.Id && c.DeletedAt == null);

            if (!canViewPrivate)
            {
                collectionsQuery = collectionsQuery.Where(c => c.Visibility == CollectionVisibility.Public);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchQuery))
            {
                collectionsQuery = collectionsQuery.Where(c => c.Name.Contains(query.SearchQuery) ||
                    (c.Description != null && c.Description.Contains(query.SearchQuery)));
            }

            var totalCount = await collectionsQuery.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var ownerSummary = new GetUserCollectionsOwnerDto
            {
                Id = targetUser.Id,
                Username = targetUser.Username,
                Avatar = targetUser.Avatar,
                ProfilePictureUrl = targetUser.ProfilePictureUrl
            };

            var items = await collectionsQuery
                .OrderBy(c => c.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new GetUserCollectionsListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Visibility = c.Visibility,
                    OwnerId = c.OwnerId,
                    Avatar = c.Avatar,
                    Favorite = c.Favorite,
                    DisplayOrder = c.DisplayOrder,
                    IsPasswordProtected = c.PasswordHash != null,
                    ModelCount = c.CollectionModels.Count(cm => cm.Model.DeletedAt == null),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.Owner = ownerSummary;
            }

            return new GetUserCollectionsResponseDto
            {
                Collections = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
