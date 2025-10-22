using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Search.Domain
{
    public class SearchResponse
    {
        public IEnumerable<SearchResultItem> Results { get; set; } = new List<SearchResultItem>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; } = string.Empty;
        public SearchType Type { get; set; }
    }

    public class SearchResultItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Avatar { get; set; }
        public SearchResultType Type { get; set; }
        public string? Author { get; set; }
        public Guid? AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Downloads { get; set; }
        public int? Likes { get; set; }
        public int? ModelCount { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public double RelevanceScore { get; set; }
    }

    public enum SearchResultType
    {
        Model,
        User,
        Collection
    }
}
