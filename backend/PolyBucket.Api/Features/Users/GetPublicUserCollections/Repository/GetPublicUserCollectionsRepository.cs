using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Repository;

public class GetPublicUserCollectionsRepository(PolyBucketDbContext dbContext) : IGetPublicUserCollectionsRepository
{
    public async Task<GetPublicUserCollectionsResult> GetPublicUserCollectionsAsync(GetPublicUserCollectionsQuery query, CancellationToken cancellationToken = default)
    {
        var targetUser = query.UserId != Guid.Empty
            ? await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken)
            : await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == query.Username, cancellationToken);

        if (targetUser == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var canViewPrivate = query.IsRequestingUserAdmin || (query.RequestingUserId.HasValue && query.RequestingUserId.Value == targetUser.Id);
        if (!targetUser.IsProfilePublic && !canViewPrivate)
        {
            throw new UnauthorizedAccessException("User profile is private");
        }

        var collectionsQuery = dbContext.Collections
            .AsNoTracking()
            .Where(c => c.OwnerId == targetUser.Id && c.DeletedAt == null);

        if (!canViewPrivate)
        {
            collectionsQuery = collectionsQuery.Where(c => c.Visibility == CollectionVisibility.Public);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            collectionsQuery = collectionsQuery.Where(c =>
                c.Name.Contains(query.SearchQuery) ||
                (c.Description != null && c.Description.Contains(query.SearchQuery)));
        }

        collectionsQuery = (query.SortBy ?? "CreatedAt").ToLowerInvariant() switch
        {
            "name" => query.SortDescending
                ? collectionsQuery.OrderByDescending(c => c.Name)
                : collectionsQuery.OrderBy(c => c.Name),
            "updatedat" => query.SortDescending
                ? collectionsQuery.OrderByDescending(c => c.UpdatedAt)
                : collectionsQuery.OrderBy(c => c.UpdatedAt),
            _ => query.SortDescending
                ? collectionsQuery.OrderByDescending(c => c.CreatedAt)
                : collectionsQuery.OrderBy(c => c.CreatedAt),
        };

        var totalCount = await collectionsQuery.CountAsync(cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;
        var items = await collectionsQuery
            .Skip(skip)
            .Take(query.PageSize)
            .Select(c => new PublicUserCollectionListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Avatar = c.Avatar,
                Visibility = c.Visibility.ToString(),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt ?? c.CreatedAt,
                ModelCount = c.CollectionModels.Count(cm => cm.Model.DeletedAt == null),
                IsPasswordProtected = c.PasswordHash != null
            })
            .ToListAsync(cancellationToken);

        return new GetPublicUserCollectionsResult
        {
            Collections = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        };
    }
}
