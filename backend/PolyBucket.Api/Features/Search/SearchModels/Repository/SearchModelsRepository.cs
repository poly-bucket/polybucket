using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Search.SearchModels.Repository;

public class SearchModelsRepository(PolyBucketDbContext context, ISearchCapabilities capabilities) : ISearchModelsRepository
{
    public async Task<SearchResponse> SearchModelsAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var response = new SearchResponse
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Query = query.Query,
            Type = SearchType.Models
        };

        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return response;
        }

        var term = SearchQueryBuilder.NormalizeTerm(query.Query);
        var mode = await capabilities.GetModeAsync(context, cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;

        var visible = context.Models
            .AsNoTracking()
            .Where(m => m.DeletedAt == null && m.Privacy == PrivacySettings.Public)
            .WherePubliclyVisible(context.ModelModerationRecords);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim().ToLower();
            visible = visible.Where(m => m.Categories.Any(c => c.Name.ToLower() == category));
        }

        var scored = SearchQueryBuilder.ScoreModels(visible, term, mode);
        var count = await scored.CountAsync(cancellationToken);
        response.Counts.Models = count;
        response.TotalCount = count;
        response.TotalPages = count == 0 ? 0 : (int)Math.Ceiling((double)count / query.PageSize);

        if (count == 0)
        {
            return response;
        }

        response.Results = await SearchQueryBuilder.OrderModels(scored, query.SortBy, query.SortDescending)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(s => new SearchResultItem
            {
                Id = s.Entity.Id,
                Title = s.Entity.Name,
                Description = s.Entity.Description,
                ThumbnailUrl = s.Entity.ThumbnailUrl,
                Type = SearchResultType.Model,
                Author = s.Entity.Author.Username,
                AuthorId = s.Entity.AuthorId,
                CreatedAt = s.Entity.CreatedAt,
                UpdatedAt = s.Entity.UpdatedAt ?? s.Entity.CreatedAt,
                Downloads = s.Entity.Downloads,
                Likes = s.Entity.Likes,
                Dislikes = s.Entity.Dislikes,
                RelevanceScore = s.Score
            })
            .ToListAsync(cancellationToken);

        return response;
    }
}
