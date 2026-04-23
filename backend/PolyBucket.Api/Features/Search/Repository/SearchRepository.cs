using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Search.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Search.Repository
{
    public class SearchRepository : ISearchRepository
    {
        private readonly PolyBucketDbContext _context;

        public SearchRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResponse> SearchAsync(SearchQuery query)
        {
            var results = new List<SearchResultItem>();
            var totalCount = 0;

            if (query.Type == SearchType.All || query.Type == SearchType.Models)
            {
                var (modelResults, modelCount) = await SearchModelsAsync(query);
                results.AddRange(modelResults);
                totalCount += modelCount;
            }

            if (query.Type == SearchType.All || query.Type == SearchType.Users)
            {
                var (userResults, userCount) = await SearchUsersAsync(query);
                results.AddRange(userResults);
                totalCount += userCount;
            }

            if (query.Type == SearchType.All || query.Type == SearchType.Collections)
            {
                var (collectionResults, collectionCount) = await SearchCollectionsAsync(query);
                results.AddRange(collectionResults);
                totalCount += collectionCount;
            }

            // Sort by relevance score
            results = results.OrderByDescending(r => r.RelevanceScore).ToList();

            // Apply pagination
            var paginatedResults = results
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            return new SearchResponse
            {
                Results = paginatedResults,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages,
                Query = query.Query,
                Type = query.Type
            };
        }

        private async Task<(List<SearchResultItem>, int)> SearchModelsAsync(SearchQuery query)
        {
            var searchTerm = query.Query.ToLower();
            
            var models = await _context.Models
                .Include(m => m.Author)
                .Include(m => m.Categories)
                .Include(m => m.Tags)
                .Where(m => m.DeletedAt == null && m.Privacy == PolyBucket.Api.Common.Models.Enums.PrivacySettings.Public)
                .ToListAsync();

            var results = models
                .Where(m => 
                    m.Name.ToLower().Contains(searchTerm) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchTerm)) ||
                    m.Categories.Any(c => c.Name.ToLower().Contains(searchTerm)) ||
                    m.Tags.Any(t => t.Name.ToLower().Contains(searchTerm)) ||
                    m.Author.Username.ToLower().Contains(searchTerm))
                .Select(m => new SearchResultItem
                {
                    Id = m.Id,
                    Title = m.Name,
                    Description = m.Description,
                    ThumbnailUrl = m.ThumbnailUrl,
                    Type = SearchResultType.Model,
                    Author = m.Author.Username,
                    AuthorId = m.AuthorId,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt ?? m.CreatedAt,
                    Downloads = m.Downloads,
                    Likes = m.Likes,
                    RelevanceScore = CalculateRelevanceScore(m.Name, m.Description, searchTerm)
                })
                .ToList();

            return (results, results.Count);
        }

        private async Task<(List<SearchResultItem>, int)> SearchUsersAsync(SearchQuery query)
        {
            var searchTerm = query.Query.ToLower();
            
            var users = await _context.Users
                .Where(u => u.DeletedAt == null && u.IsProfilePublic)
                .ToListAsync();

            var results = users
                .Where(u => 
                    u.Username.ToLower().Contains(searchTerm) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
                    (u.Bio != null && u.Bio.ToLower().Contains(searchTerm)))
                .Select(u => new SearchResultItem
                {
                    Id = u.Id,
                    Title = u.Username,
                    Description = u.Bio,
                    Avatar = u.Avatar,
                    Type = SearchResultType.User,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt ?? u.CreatedAt,
                    RelevanceScore = CalculateRelevanceScore(u.Username, u.Bio, searchTerm)
                })
                .ToList();

            return (results, results.Count);
        }

        private async Task<(List<SearchResultItem>, int)> SearchCollectionsAsync(SearchQuery query)
        {
            var searchTerm = query.Query.ToLower();
            
            var collections = await _context.Collections
                .Include(c => c.Owner)
                .Include(c => c.CollectionModels)
                .Where(c => c.DeletedAt == null && c.Visibility == PolyBucket.Api.Features.Collections.Domain.Enums.CollectionVisibility.Public)
                .ToListAsync();

            var results = collections
                .Where(c => 
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
                    c.Owner.Username.ToLower().Contains(searchTerm))
                .Select(c => new SearchResultItem
                {
                    Id = c.Id,
                    Title = c.Name,
                    Description = c.Description,
                    Avatar = c.Avatar,
                    Type = SearchResultType.Collection,
                    Author = c.Owner.Username,
                    AuthorId = c.OwnerId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt ?? c.CreatedAt,
                    ModelCount = c.CollectionModels.Count,
                    RelevanceScore = CalculateRelevanceScore(c.Name, c.Description, searchTerm)
                })
                .ToList();

            return (results, results.Count);
        }

        private double CalculateRelevanceScore(string title, string? description, string searchTerm)
        {
            var score = 0.0;
            var titleLower = title.ToLower();
            var descriptionLower = description?.ToLower() ?? string.Empty;

            // Exact match in title gets highest score
            if (titleLower == searchTerm)
                score += 100;
            else if (titleLower.StartsWith(searchTerm))
                score += 80;
            else if (titleLower.Contains(searchTerm))
                score += 60;

            // Description matches get lower score
            if (descriptionLower.Contains(searchTerm))
                score += 30;

            // Fuzzy matching for typos (simple implementation)
            score += CalculateFuzzyScore(titleLower, searchTerm) * 10;
            if (!string.IsNullOrEmpty(description))
                score += CalculateFuzzyScore(descriptionLower, searchTerm) * 5;

            return score;
        }

        private double CalculateFuzzyScore(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
                return 0;

            var distance = LevenshteinDistance(text, searchTerm);
            var maxLength = Math.Max(text.Length, searchTerm.Length);
            
            if (maxLength == 0)
                return 1;

            return 1 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }
    }
}
