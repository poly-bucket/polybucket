using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Search.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Search.Repository
{
    public class SearchRepository : ISearchRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ISearchCapabilities _capabilities;
        private readonly SearchModels.Repository.ISearchModelsRepository _searchModelsRepository;

        public SearchRepository(
            PolyBucketDbContext context,
            ISearchCapabilities capabilities,
            SearchModels.Repository.ISearchModelsRepository searchModelsRepository)
        {
            _context = context;
            _capabilities = capabilities;
            _searchModelsRepository = searchModelsRepository;
        }

        public async Task<SearchResponse> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            var response = new SearchResponse
            {
                Page = query.Page,
                PageSize = query.PageSize,
                Query = query.Query,
                Type = query.Type
            };

            if (string.IsNullOrWhiteSpace(query.Query))
            {
                return response;
            }

            var term = SearchQueryBuilder.NormalizeTerm(query.Query);
            var mode = await _capabilities.GetModeAsync(_context, cancellationToken);
            var results = new List<SearchResultItem>();

            if (query.Type is SearchType.All or SearchType.Models)
            {
                var modelsResponse = await _searchModelsRepository.SearchModelsAsync(query, cancellationToken);
                results.AddRange(modelsResponse.Results);
                response.Counts.Models = modelsResponse.Counts.Models;
            }

            if (query.Type is SearchType.All or SearchType.Users)
            {
                var (items, count) = await SearchUsersAsync(query, term, mode, cancellationToken);
                results.AddRange(items);
                response.Counts.Users = count;
            }

            if (query.Type is SearchType.All or SearchType.Collections)
            {
                var (items, count) = await SearchCollectionsAsync(query, term, mode, cancellationToken);
                results.AddRange(items);
                response.Counts.Collections = count;
            }

            var largestTypeCount = Math.Max(response.Counts.Models, Math.Max(response.Counts.Users, response.Counts.Collections));
            response.Results = results;
            response.TotalCount = response.Counts.Models + response.Counts.Users + response.Counts.Collections;
            response.TotalPages = (int)Math.Ceiling((double)largestTypeCount / query.PageSize);
            return response;
        }

        private int Skip(SearchQuery query) => (query.Page - 1) * query.PageSize;

        private async Task<(List<SearchResultItem>, int)> SearchUsersAsync(SearchQuery query, string term, SearchTextMode mode, CancellationToken cancellationToken)
        {
            var visible = _context.Users
                .AsNoTracking()
                .Where(u => u.DeletedAt == null && u.IsProfilePublic);

            var scored = SearchQueryBuilder.ScoreUsers(visible, term, mode);
            var count = await scored.CountAsync(cancellationToken);
            if (count == 0)
            {
                return (new List<SearchResultItem>(), 0);
            }

            var items = await SearchQueryBuilder.OrderUsers(scored, query.SortBy, query.SortDescending)
                .Skip(Skip(query))
                .Take(query.PageSize)
                .Select(s => new SearchResultItem
                {
                    Id = s.Entity.Id,
                    Title = s.Entity.Username,
                    Description = s.Entity.Bio,
                    Avatar = s.Entity.Avatar,
                    Type = SearchResultType.User,
                    Username = s.Entity.Username,
                    Email = s.Entity.ShowEmail ? s.Entity.Email : null,
                    CreatedAt = s.Entity.CreatedAt,
                    UpdatedAt = s.Entity.UpdatedAt ?? s.Entity.CreatedAt,
                    RelevanceScore = s.Score
                })
                .ToListAsync(cancellationToken);

            return (items, count);
        }

        private async Task<(List<SearchResultItem>, int)> SearchCollectionsAsync(SearchQuery query, string term, SearchTextMode mode, CancellationToken cancellationToken)
        {
            var visible = _context.Collections
                .AsNoTracking()
                .Where(c => c.DeletedAt == null && c.Visibility == CollectionVisibility.Public);

            var scored = SearchQueryBuilder.ScoreCollections(visible, term, mode);
            var count = await scored.CountAsync(cancellationToken);
            if (count == 0)
            {
                return (new List<SearchResultItem>(), 0);
            }

            var items = await SearchQueryBuilder.OrderCollections(scored, query.SortBy, query.SortDescending)
                .Skip(Skip(query))
                .Take(query.PageSize)
                .Select(s => new SearchResultItem
                {
                    Id = s.Entity.Id,
                    Title = s.Entity.Name,
                    Description = s.Entity.Description,
                    Avatar = s.Entity.Avatar,
                    Type = SearchResultType.Collection,
                    Author = s.Entity.Owner.Username,
                    AuthorId = s.Entity.OwnerId,
                    CreatedAt = s.Entity.CreatedAt,
                    UpdatedAt = s.Entity.UpdatedAt ?? s.Entity.CreatedAt,
                    ModelCount = s.Entity.CollectionModels.Count,
                    RelevanceScore = s.Score
                })
                .ToListAsync(cancellationToken);

            return (items, count);
        }
    }
}
