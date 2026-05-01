using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Users.GetUserModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserModels.Repository;

public class GetUserModelsRepository(PolyBucketDbContext dbContext) : IGetUserModelsRepository
{
    public async Task<GetUserModelsResult> GetUserPublicModelsAsync(GetUserModelsQuery query, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == query.Username, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with username {query.Username} not found");
        }

        var canViewPrivate = query.IsRequestingUserAdmin || (query.RequestingUserId.HasValue && query.RequestingUserId.Value == user.Id);
        var includePrivate = query.IncludePrivate || canViewPrivate;

        if (!user.IsProfilePublic)
        {
            if (!canViewPrivate)
            {
                throw new UnauthorizedAccessException("User profile is private");
            }
        }

        var modelsQuery = dbContext.Models
            .Where(m => m.AuthorId == user.Id && m.DeletedAt == null);

        if (!includePrivate)
        {
            modelsQuery = modelsQuery.Where(m => m.Privacy == PrivacySettings.Public);
        }

        var totalCount = await modelsQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
        var skip = (query.Page - 1) * query.PageSize;

        var models = await modelsQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Description,
                m.ThumbnailUrl,
                m.Downloads,
                m.Likes,
                m.CreatedAt,
                m.UpdatedAt,
                m.License,
                m.AIGenerated,
                m.WIP,
                m.NSFW,
                m.IsRemix,
                m.Privacy
            })
            .ToListAsync(cancellationToken);

        var userModels = models.Select(m => new UserModelListItemDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            ThumbnailUrl = m.ThumbnailUrl,
            Downloads = m.Downloads,
            Likes = m.Likes,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt ?? m.CreatedAt,
            License = m.License?.ToString(),
            AIGenerated = m.AIGenerated,
            WIP = m.WIP,
            NSFW = m.NSFW,
            IsRemix = m.IsRemix,
            Privacy = m.Privacy
        }).ToList();

        return new GetUserModelsResult
        {
            Models = userModels,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }
}
