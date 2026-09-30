using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;

public class GetUserLikedModelsRepository(PolyBucketDbContext dbContext) : IGetUserLikedModelsRepository
{
    public async Task<GetUserLikedModelsResult> GetUserLikedModelsAsync(GetUserLikedModelsQuery query, CancellationToken cancellationToken = default)
    {
        var likesQuery =
            from like in dbContext.Likes.AsNoTracking()
            join model in dbContext.Models.AsNoTracking() on like.ModelId equals model.Id
            join author in dbContext.Users.AsNoTracking() on model.AuthorId equals author.Id
            where like.UserId == query.UserId
                  && like.DeletedAt == null
                  && like.Type == ModelReactionType.Like
                  && model.DeletedAt == null
                  && model.Privacy == PrivacySettings.Public
            select new { like, model, author };

        if (!string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            var search = query.SearchQuery.Trim();
            likesQuery = likesQuery.Where(x =>
                x.model.Name.Contains(search) ||
                x.model.Description.Contains(search));
        }

        var sortBy = query.SortBy ?? "LikedAt";
        likesQuery = sortBy switch
        {
            "Name" => query.SortDescending
                ? likesQuery.OrderByDescending(x => x.model.Name)
                : likesQuery.OrderBy(x => x.model.Name),
            "Downloads" => query.SortDescending
                ? likesQuery.OrderByDescending(x => x.model.Downloads)
                : likesQuery.OrderBy(x => x.model.Downloads),
            "Likes" => query.SortDescending
                ? likesQuery.OrderByDescending(x => x.model.Likes)
                : likesQuery.OrderBy(x => x.model.Likes),
            "CreatedAt" => query.SortDescending
                ? likesQuery.OrderByDescending(x => x.model.CreatedAt)
                : likesQuery.OrderBy(x => x.model.CreatedAt),
            _ => query.SortDescending
                ? likesQuery.OrderByDescending(x => x.like.CreatedAt)
                : likesQuery.OrderBy(x => x.like.CreatedAt)
        };

        var totalCount = await likesQuery.CountAsync(cancellationToken);
        var totalPages = query.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / query.PageSize)
            : 0;
        var skip = (query.Page - 1) * query.PageSize;

        var page = await likesQuery
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var models = page.Select(x => new UserLikedModelListItemDto
        {
            Id = x.model.Id,
            Name = x.model.Name,
            Description = x.model.Description,
            ThumbnailUrl = x.model.ThumbnailUrl,
            Downloads = x.model.Downloads,
            Likes = x.model.Likes,
            CreatedAt = x.model.CreatedAt,
            UpdatedAt = x.model.UpdatedAt ?? x.model.CreatedAt,
            LikedAt = x.like.CreatedAt,
            License = x.model.License?.ToString(),
            AIGenerated = x.model.AIGenerated,
            WIP = x.model.WIP,
            NSFW = x.model.NSFW,
            Author = new LikedModelAuthorDto
            {
                Id = x.author.Id,
                Username = x.author.Username,
                Avatar = x.author.Avatar
            }
        }).ToList();

        return new GetUserLikedModelsResult
        {
            Models = models,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }
}
